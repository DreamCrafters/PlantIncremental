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
    private readonly EconomyService _economyService;
    private readonly SaveService _saveService;
    private readonly GameSettings _settings;

    private readonly CompositeDisposable _disposables = new();

    [Inject]
    public GameInitializer(
        EconomyService economyService,
        SaveService saveService,
        GameSettings settings)
    {
        _economyService = economyService;
        _saveService = saveService;
        _settings = settings;
    }

    public void Initialize()
    {
        Debug.Log("=== Game Initializer Started ===");

        // Настраиваем DOTween
        InitializeDOTween();

        // Загружаем сохранение или создаем новую игру
        LoadGame();

        // Настраиваем автосохранение
        SetupAutoSave();

        Debug.Log("=== Game Initialized Successfully ===");
    }

    public void Dispose()
    {
        // Сохраняем перед выходом
        SaveGame();

        _disposables?.Dispose();
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

        // Автоматически воспроизводим твины
        DOTween.defaultAutoPlay = AutoPlay.All;

        // Используем safe mode для предотвращения ошибок, но с минимальным логированием
        DOTween.logBehaviour = LogBehaviour.ErrorsOnly;

        // Инициализируем DOTween с улучшенными настройками безопасности
        DOTween.Init(recycleAllByDefault: true, useSafeMode: true, logBehaviour: LogBehaviour.ErrorsOnly)
            .SetCapacity(500, 50);
    }

    /// <summary>
    /// Загружает сохранение или создает новую игру
    /// </summary>
    private void LoadGame()
    {
        var saveData = _saveService.Load();
        _economyService.LoadData(saveData.GetEconomyData());
    }

    /// <summary>
    /// Настраивает автосохранение
    /// </summary>
    private void SetupAutoSave()
    {
        // Автосохранение каждые N секунд
        Observable.Interval(TimeSpan.FromSeconds(_settings.GameplaySettings?.AutoSaveInterval ?? 30f))
            .Subscribe(_ => SaveGame())
            .AddTo(_disposables);

        // Сохранение при важных событиях
        _economyService.Coins
            .Skip(1) // Пропускаем начальное значение
            .Throttle(TimeSpan.FromSeconds(2)) // Не чаще чем раз в 2 секунды
            .Subscribe(_ => SaveGame())
            .AddTo(_disposables);
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
        saveData.SetEconomyData(_economyService.GetSaveData());

        return saveData;
    }
}