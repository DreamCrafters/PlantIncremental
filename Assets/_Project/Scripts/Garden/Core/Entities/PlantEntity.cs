using System;
using UniRx;
using UnityEngine;

/// <summary>
/// Основная сущность растения, управляющая состоянием и визуализацией
/// </summary>
public class PlantEntity : IPlantEntity, IDisposable
{
    private readonly ReactiveProperty<float> _growthProgress = new(0f);
    private readonly ReactiveProperty<PlantState> _state = new(PlantState.New);
    private readonly ReactiveProperty<bool> _isWaitingForWater = new(true);
    private readonly CompositeDisposable _disposables = new();

    private readonly GameSettings _gameSettings;
    private readonly PlantView _view;

    // Кэшируем для оптимизации
    private float _currentGrowthTime;
    private float _growthSpeedModifier = 1f;

    // Система полива
    private float _lastWateringTime;
    private IDisposable _witherTimer;
    private IDisposable _growthTimer; // Таймер для отслеживания роста

    private const float WITHER_TIMEOUT = 10f; // 10 секунд до увядания без полива

    public PlantData Data { get; }
    public IReadOnlyReactiveProperty<float> GrowthProgress => _growthProgress;
    public IReadOnlyReactiveProperty<PlantState> State => _state;
    public PlantView View => _view;
    public bool IsWaitingForWater => _isWaitingForWater.Value;
    public bool IsHarvestable => _state.Value == PlantState.FullyGrown;
    public bool IsWithered => _state.Value == PlantState.Withered;
    public Vector2 Position => _view.transform.position;

    // Новые свойства для полива
    public float TimeSinceLastWatering => Time.time - _lastWateringTime;

    public PlantEntity(PlantData data, PlantView view, GameSettings gameSettings)
    {
        Data = data;
        _view = view;
        _gameSettings = gameSettings;
        _lastWateringTime = Time.time;

        SubscribeToStateChanges();
        UpdateVisual();

        // Запускаем таймер увядания с самого начала
        StartWitherTimer();
    }

    public void Dispose()
    {
        _witherTimer?.Dispose();
        _growthTimer?.Dispose();
        _disposables?.Dispose();
        if (_view != null)
        {
            UnityEngine.Object.Destroy(_view.gameObject);
        }
    }

    /// <summary>
    /// Поливает растение, позволяя ему продолжить рост
    /// </summary>
    public void Water()
    {
        if (!_isWaitingForWater.Value) return;

        _lastWateringTime = Time.time;
        _isWaitingForWater.Value = false;

        // Останавливаем таймер увядания
        _witherTimer?.Dispose();
        _witherTimer = null;

        // Скрываем эффект ожидания полива
        _view.HideWateringIcon();

        _state.Value += 1;
        UpdateVisualStage();
        StartGrowing(_growthSpeedModifier);
    }

    /// <summary>
    /// Начинает процесс роста растения
    /// </summary>
    public void StartGrowing(float growthModifier = 1f)
    {
        if (_state.Value == PlantState.Withered) return;

        _growthSpeedModifier = growthModifier;

        // Останавливаем предыдущий рост, если он был
        _growthTimer?.Dispose();

        // Подписываемся на каждую секунду для обновления роста (на главном потоке)
        _growthTimer = Observable.Interval(TimeSpan.FromSeconds(1))
            .ObserveOnMainThread()
            .TakeWhile(_ => _state.Value != PlantState.FullyGrown && _state.Value != PlantState.Withered)
            .Subscribe(_ => UpdateGrowth());
    }

    /// <summary>
    /// Собирает растение и возвращает награду
    /// </summary>
    public PlantHarvestResult Harvest()
    {
        if (IsHarvestable == false)
        {
            return new PlantHarvestResult { Success = false };
        }

        var result = new PlantHarvestResult
        {
            Success = true,
            Coins = CalculateCoinsReward(),
            Petals = CalculatePetalsReward(),
        };

        // Анимация сбора
        _view.PlayHarvestAnimation();

        return result;
    }

    /// <summary>
    /// Активирует пассивную способность растения
    /// </summary>
    public void ActivatePassiveAbility()
    {
        if (_state.Value != PlantState.FullyGrown) return;

        _view.ShowPassiveEffect();
    }

    /// <summary>
    /// Останавливает процесс роста (и очищает подписки)
    /// </summary>
    public void StopGrowing()
    {
        // Останавливаем таймер роста
        _growthTimer?.Dispose();
        _growthTimer = null;
    }

    private void UpdateGrowth()
    {
        if (_growthSpeedModifier <= 0f || _isWaitingForWater.Value)
        {
            return;
        }

        // Увеличиваем прогресс роста
        var growthIncrement = 1f / Data.GrowthTime * _growthSpeedModifier;
        _currentGrowthTime += growthIncrement;
        _growthProgress.Value = Mathf.Clamp01(_currentGrowthTime);

        // Проверяем, нужен ли полив на новой стадии
        CheckForWateringNeeds();
    }

    private void CheckForWateringNeeds()
    {
        var progress = _growthProgress.Value;
        var newStage = GetGrowthStageFromProgress(progress);

        // Если мы перешли на новую стадию и это не финальная стадия
        if (newStage > _state.Value)
        {
            RequireWatering();
        }
    }

    private PlantState GetGrowthStageFromProgress(float progress)
    {
        if (progress < 0.5) return PlantState.Seed;
        if (progress < 1) return PlantState.Growing;
        return PlantState.FullyGrown;
    }

    private void RequireWatering()
    {
        _isWaitingForWater.Value = true;

        // Запускаем таймер увядания
        StartWitherTimer();
    }

    private void StartWitherTimer()
    {
        _witherTimer?.Dispose();

        _witherTimer = Observable.Timer(TimeSpan.FromSeconds(WITHER_TIMEOUT))
            .Subscribe(_ =>
            {
                if (_isWaitingForWater.Value)
                {
                    _state.Value = PlantState.Withered;
                }
            });
    }

    private void SubscribeToStateChanges()
    {
        _state.Subscribe(state =>
        {
            UpdateVisual();

            // Обновляем иконки в зависимости от состояния
            switch (state)
            {
                case PlantState.FullyGrown:
                    OnFullyGrown();
                    break;

                case PlantState.Withered:
                    OnWithered();
                    break;

                case PlantState.Growing:
                case PlantState.Seed:
                    // Убираем специальные иконки только если действительно не ждем полив
                    if (!_isWaitingForWater.Value)
                    {
                        _view.HideWateringIcon();
                    }
                    break;
            }
        }).AddTo(_disposables);

        _isWaitingForWater.Subscribe(isWaiting =>
        {
            if (isWaiting)
            {
                OnWaitingForWater();
            }
        }).AddTo(_disposables);
    }

    private void UpdateVisual()
    {
        // Обновляем визуал только на основе текущей визуальной стадии, а не состояния
        UpdateVisualStage();
    }

    private void UpdateVisualStage()
    {
        if (_view == null || Data.GrowthStages == null) return;

        _view.UpdateSprite(Data.GrowthStages[(int)_state.Value].Sprite);
    }

    private void OnFullyGrown()
    {
        // Активируем пассивную способность
        ActivatePassiveAbility();
    }

    private void OnWithered()
    {
        _view.PlayWitherEffect();
        StopGrowing();

        // Останавливаем таймер увядания
        _witherTimer?.Dispose();
        _witherTimer = null;
    }

    private void OnWaitingForWater()
    {
        // Показываем иконку полива
        _view.ShowWateringIcon();
    }

    private int CalculateCoinsReward()
    {
        return Data.SellPrice;
    }

    private PetalData CalculatePetalsReward()
    {
        return new PetalData(Data.Type, 1);
    }
}
