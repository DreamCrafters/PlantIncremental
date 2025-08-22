using System;
using UniRx;
using UnityEngine;

/// <summary>
/// Основная сущность растения, управляющая состоянием и визуализацией
/// </summary>
public class PlantEntity : IPlantEntity, IDisposable
{
    private readonly ReactiveProperty<float> _growthProgress = new(0f);
    private readonly ReactiveProperty<PlantState> _state = new(PlantState.Seed);
    private readonly CompositeDisposable _disposables = new();

    private readonly GameSettings _gameSettings;
    private readonly PlantView _view;

    // Кэшируем для оптимизации
    private float _currentGrowthTime;
    private float _growthSpeedModifier = 1f;
    private bool _isWithering;
    private float _witherChance;

    public PlantData Data { get; }
    public IReadOnlyReactiveProperty<float> GrowthProgress => _growthProgress;
    public IReadOnlyReactiveProperty<PlantState> State => _state;
    public PlantView View => _view;
    public bool IsHarvestable => _state.Value == PlantState.FullyGrown;
    public bool IsWithered => _state.Value == PlantState.Withered;
    public Vector2 Position => _view.transform.position;

    public PlantEntity(PlantData data, PlantView view, GameSettings gameSettings)
    {
        Data = data;
        _view = view;
        _gameSettings = gameSettings;

        InitializeWitherChance();
        SubscribeToStateChanges();
        UpdateVisual();
    }

    public void Dispose()
    {
        _disposables?.Dispose();
        if (_view != null)
        {
            UnityEngine.Object.Destroy(_view.gameObject);
        }
    }

    /// <summary>
    /// Начинает процесс роста растения
    /// </summary>
    public void StartGrowing(float growthModifier = 1f)
    {
        if (_state.Value == PlantState.Withered) return;

        _growthSpeedModifier = growthModifier;

        // Подписываемся на каждую секунду для обновления роста (на главном потоке)
        Observable.Interval(TimeSpan.FromSeconds(1))
            .ObserveOnMainThread()
            .TakeWhile(_ => _state.Value != PlantState.FullyGrown && _state.Value != PlantState.Withered)
            .Subscribe(_ => UpdateGrowth())
            .AddTo(_disposables);
    }

    /// <summary>
    /// Применяет модификатор скорости роста (от соседних растений или почвы)
    /// </summary>
    public void ApplyGrowthModifier(float modifier)
    {
        _growthSpeedModifier = Mathf.Max(0f, modifier);
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

        // Начинаем проверку на увядание
        StartWitherCheck();

        // Активируем способность в зависимости от типа
        // Это будет расширено в PlantAbilitySystem
        _view.ShowPassiveEffect();
    }

    /// <summary>
    /// Останавливает процесс роста (и очищает подписки)
    /// </summary>
    public void StopGrowing()
    {
        // Отписываемся от обновлений роста
        _disposables.Clear();
    }

    private void UpdateGrowth()
    {
        if (_growthSpeedModifier <= 0f) return;

        // Увеличиваем прогресс роста
        var growthIncrement = 1f / Data.GrowthTime * _growthSpeedModifier;
        _currentGrowthTime += growthIncrement;
        _growthProgress.Value = Mathf.Clamp01(_currentGrowthTime);

        // Обновляем состояние в зависимости от прогресса
        UpdateStateByProgress();
    }

    private void UpdateStateByProgress()
    {
        var progress = _growthProgress.Value;
        // Быстрый путь: если уже полностью выросло
        if (progress >= 1f)
        {
            if (_state.Value != PlantState.FullyGrown)
                _state.Value = PlantState.FullyGrown;
            return;
        }

        var newState = progress switch
        {
            < 0.5f => PlantState.Seed,
            < 1f => PlantState.Growing,
            _ => PlantState.FullyGrown
        };

        if (_state.Value != newState)
        {
            _state.Value = newState;
        }
    }

    private void SubscribeToStateChanges()
    {
        _state.Subscribe(state =>
        {
            UpdateVisual();

            if (state == PlantState.FullyGrown)
            {
                OnFullyGrown();
            }
            else if (state == PlantState.Withered)
            {
                OnWithered();
            }
        }).AddTo(_disposables);
    }

    private void UpdateVisual()
    {
        if (_view == null || Data.GrowthStages == null) return;

        var stageIndex = GetSpriteIndexForState(_state.Value);
        if (stageIndex < Data.GrowthStages.Length)
        {
            _view.UpdateSprite(Data.GrowthStages[stageIndex].Sprite);
        }

        // Обновляем цвет в зависимости от состояния
        if (_state.Value == PlantState.Withered)
        {
            _view.SetWitheredVisual();
        }
    }

    private int GetSpriteIndexForState(PlantState state)
    {
        return (int)state;
    }

    private void OnFullyGrown()
    {
        // Визуальный эффект созревания
        _view.PlayGrowthCompleteEffect();

        // Активируем пассивную способность
        ActivatePassiveAbility();
    }

    private void OnWithered()
    {
        _view.PlayWitherEffect();
        StopGrowing();
    }

    private void StartWitherCheck()
    {
        if (_isWithering) return;
        _isWithering = true;

        // Проверяем шанс увядания каждые 1 секунд (на главном потоке)
        Observable.Interval(TimeSpan.FromSeconds(1))
            .ObserveOnMainThread()
            .TakeWhile(_ => _state.Value == PlantState.FullyGrown)
            .Subscribe(_ => CheckWither())
            .AddTo(_disposables);
    }

    private void CheckWither()
    {
        var random = UnityEngine.Random.Range(0f, 1f);
        if (random < _witherChance)
        {
            _state.Value = PlantState.Withered;
        }
    }

    private void InitializeWitherChance()
    {
        _witherChance = _gameSettings.WitherChancePerSecond;
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