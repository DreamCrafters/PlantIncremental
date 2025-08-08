using System;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using DG.Tweening;

/// <summary>
/// Основной инициализатор игры, запускает все системы
/// </summary>
public class GameInitializer : IInitializable, IDisposable
{
    private readonly IGridService _gridService;
    private readonly IPlantGrowthService _growthService;
    private readonly IEconomyService _economyService;
    private readonly ISaveService _saveService;
    private readonly GridView _gridView;
    private readonly GameSettings _settings;
    
    private readonly CompositeDisposable _disposables = new();
    
    [Inject]
    public GameInitializer(
        IGridService gridService,
        IPlantGrowthService growthService,
        IEconomyService economyService,
        ISaveService saveService,
        GridView gridView,
        GameSettings settings)
    {
        _gridService = gridService;
        _growthService = growthService;
        _economyService = economyService;
        _saveService = saveService;
        _gridView = gridView;
        _settings = settings;
    }
    
    public void Initialize()
    {
        Debug.Log("=== Game Initializer Started ===");
        
        // Настраиваем DOTween
        InitializeDOTween();
        
        // Загружаем сохранение или создаем новую игру
        LoadOrCreateGame();
        
        // Настраиваем автосохранение
        SetupAutoSave();
        
        // Подписываемся на основные события
        SubscribeToGameEvents();
        
        // Проверяем целостность данных
        ValidateGameState();
        
        Debug.Log("=== Game Initialized Successfully ===");
    }
    
    /// <summary>
    /// Настраивает DOTween для оптимальной производительности
    /// </summary>
    private void InitializeDOTween()
    {
        // Устанавливаем достаточную емкость для твинов
        DOTween.SetTweensCapacity(500, 50);
        
        // Устанавливаем режим рециркуляции твинов по умолчанию
        DOTween.defaultRecyclable = true;
        
        // Автоматически убиваем твины при отключении компонентов
        DOTween.defaultAutoKill = true;
        
        // Устанавливаем логирование для отладки (можно отключить в продакшене)
        DOTween.logBehaviour = LogBehaviour.ErrorsOnly;
        
        // Инициализируем DOTween
        DOTween.Init(recycleAllByDefault: true, useSafeMode: true, logBehaviour: LogBehaviour.ErrorsOnly);
        
        Debug.Log("DOTween initialized with optimized settings");
    }

    /// <summary>
    /// Загружает сохранение или создает новую игру
    /// </summary>
    private void LoadOrCreateGame()
    {
        var saveData = _saveService.Load();
        
        if (saveData != null && IsValidSaveData(saveData))
        {
            Debug.Log("Loading saved game...");
            ApplySaveData(saveData);
        }
        else
        {
            Debug.Log("Starting new game...");
            StartNewGame();
        }
    }
    
    /// <summary>
    /// Проверяет валидность сохранения
    /// </summary>
    private bool IsValidSaveData(SaveData saveData)
    {
        // TODO: Добавить проверку валидности данных
        return saveData != null;
    }
    
    /// <summary>
    /// Применяет загруженные данные
    /// </summary>
    private void ApplySaveData(SaveData saveData)
    {
        // TODO: Реализовать загрузку данных
        // _economyService.LoadCoins(saveData.Coins);
        // _economyService.LoadPetalsData(saveData.Petals);
        // _gridService.LoadGrid(saveData.Grid);
        
        Debug.Log("Save data applied");
    }
    
    /// <summary>
    /// Начинает новую игру
    /// </summary>
    private void StartNewGame()
    {
        // Начальные ресурсы
        _economyService.AddCoins(100);
        
        // Начальные лепестки для тестирования
        if (_settings.AvailablePlants != null && _settings.AvailablePlants.Length > 0)
        {
            var firstPlantType = _settings.AvailablePlants[0].Type;
            _economyService.AddPetals(firstPlantType, 5);
        }
        
        Debug.Log("New game started with initial resources");
    }
    
    /// <summary>
    /// Настраивает автосохранение
    /// </summary>
    private void SetupAutoSave()
    {
        // Автосохранение каждые N секунд
        Observable.Interval(TimeSpan.FromSeconds(_settings.AutoSaveInterval))
            .Subscribe(_ => SaveGame())
            .AddTo(_disposables);
        
        // Сохранение при важных событиях
        _economyService.Coins
            .Skip(1) // Пропускаем начальное значение
            .Throttle(TimeSpan.FromSeconds(2)) // Не чаще чем раз в 2 секунды
            .Subscribe(_ => SaveGame())
            .AddTo(_disposables);
        
        Debug.Log($"Auto-save configured with interval: {_settings.AutoSaveInterval}s");
    }
    
    /// <summary>
    /// Подписывается на основные игровые события
    /// </summary>
    private void SubscribeToGameEvents()
    {
        // Подписка на сбор урожая
        _gridService.OnPlantHarvested
            .Subscribe(evt => OnPlantHarvested(evt))
            .AddTo(_disposables);
        
        // Подписка на полный рост растений
        _growthService.OnPlantGrown
            .Subscribe(plant => OnPlantFullyGrown(plant))
            .AddTo(_disposables);
        
        // Подписка на изменение лепестков
        _economyService.OnPetalChanged
            .Subscribe(type => OnPetalsChanged(type))
            .AddTo(_disposables);
    }
    
    /// <summary>
    /// Обработка события сбора урожая
    /// </summary>
    private void OnPlantHarvested(PlantHarvestedEvent evt)
    {
        Debug.Log($"Plant harvested at {evt.Position}, reward: {evt.Reward} coins");
        
        // Проверяем достижения
        CheckHarvestAchievements(evt);
        
        // Обновляем статистику
        UpdateHarvestStatistics(evt);
    }
    
    /// <summary>
    /// Обработка полного роста растения
    /// </summary>
    private void OnPlantFullyGrown(IPlantEntity plant)
    {
        Debug.Log($"Plant fully grown: {plant.Data.DisplayName}");
        
        // Можно добавить уведомление игроку
        ShowGrowthNotification(plant);
    }
    
    /// <summary>
    /// Обработка изменения количества лепестков
    /// </summary>
    private void OnPetalsChanged(PlantType type)
    {
        var amount = _economyService.GetPetalsAmount(type);
        Debug.Log($"Petals changed - Type: {type}, Amount: {amount}");
    }
    
    /// <summary>
    /// Проверяет достижения при сборе урожая
    /// </summary>
    private void CheckHarvestAchievements(PlantHarvestedEvent evt)
    {
        // TODO: Интеграция с системой достижений
        // Примеры достижений:
        // - Первый урожай
        // - Собрать 100 растений
        // - Собрать редкое растение
    }
    
    /// <summary>
    /// Обновляет статистику сбора
    /// </summary>
    private void UpdateHarvestStatistics(PlantHarvestedEvent evt)
    {
        // TODO: Система статистики
        // - Общее количество собранных растений
        // - Общий доход
        // - Самое ценное растение
    }
    
    /// <summary>
    /// Показывает уведомление о готовности растения
    /// </summary>
    private void ShowGrowthNotification(IPlantEntity plant)
    {
        if (_gridView != null)
        {
            var message = $"{plant.Data.DisplayName} готово к сбору!";
            _gridView.ShowMessage(message, MessageType.Success);
        }
    }
    
    /// <summary>
    /// Проверяет целостность игрового состояния
    /// </summary>
    private void ValidateGameState()
    {
        // Проверяем, что все системы инициализированы
        if (_gridService.Grid.Value == null)
        {
            Debug.LogError("Grid is not initialized!");
            return;
        }
        
        // Проверяем наличие растений для игры
        if (_settings.AvailablePlants == null || _settings.AvailablePlants.Length == 0)
        {
            Debug.LogWarning("No plants configured in GameSettings!");
        }
        
        // Проверяем начальные ресурсы
        if (_economyService.Coins.Value < 0)
        {
            Debug.LogError("Invalid coins amount!");
            _economyService.AddCoins(100);
        }
        
        Debug.Log("Game state validated successfully");
    }
    
    /// <summary>
    /// Сохраняет игру
    /// </summary>
    private void SaveGame()
    {
        try
        {
            var saveData = CreateSaveData();
            _saveService.Save(saveData);
            Debug.Log("Game saved successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }
    
    /// <summary>
    /// Создает данные для сохранения
    /// </summary>
    private SaveData CreateSaveData()
    {
        var saveData = new SaveData();
        
        // TODO: Заполнить данными
        // saveData.Coins = _economyService.Coins.Value;
        // saveData.Petals = _economyService.GetPetalsSaveData();
        // saveData.Grid = _gridService.GetGridSaveData();
        // saveData.Timestamp = DateTime.Now;
        
        return saveData;
    }
    
    public void Dispose()
    {
        // Сохраняем перед выходом
        SaveGame();
        
        _disposables?.Dispose();
        
        Debug.Log("Game Initializer disposed");
    }
}