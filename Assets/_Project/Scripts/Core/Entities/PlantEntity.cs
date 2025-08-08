using System;
using UniRx;
using UnityEngine;
using VContainer;

/// <summary>
/// Основная сущность растения, управляющая состоянием и визуализацией
/// </summary>
public class PlantEntity : IPlantEntity, IDisposable
{
   [Inject] private readonly ITimeService _timeService;

    private readonly ReactiveProperty<float> _growthProgress = new(0f);
    private readonly ReactiveProperty<PlantState> _state = new(PlantState.Seed);
    private readonly CompositeDisposable _disposables = new();

    private readonly PlantView _view;

    // Кэшируем для оптимизации
    private float _currentGrowthTime;
    private float _growthSpeedModifier = 1f;
    private bool _isWithering;
    private float _witherChance;

    public PlantData Data { get; }
    public IReadOnlyReactiveProperty<float> GrowthProgress => _growthProgress;
    public IReadOnlyReactiveProperty<PlantState> State => _state;

    // Дополнительные свойства для геймплея
    public bool IsHarvestable => _state.Value == PlantState.FullyGrown;
    public bool IsWithered => _state.Value == PlantState.Withered;
    public Vector2 Position => _view.transform.position;

    [Inject]
    public PlantEntity(PlantData data, PlantView view)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        _view = view ?? throw new ArgumentNullException(nameof(view));

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

        // Подписываемся на каждую секунду для обновления роста
        _timeService.EverySecond
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
            BonusItems = GenerateBonusItems()
        };

        // Анимация сбора
        PlayHarvestAnimation();

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

    private void UpdateGrowth()
    {
        if (_growthSpeedModifier <= 0f) return;

        // Увеличиваем прогресс роста
        var growthIncrement = (1f / Data.GrowthTime) * _growthSpeedModifier;
        _currentGrowthTime += growthIncrement;
        _growthProgress.Value = Mathf.Clamp01(_currentGrowthTime);

        // Обновляем состояние в зависимости от прогресса
        UpdateStateByProgress();
    }

    private void UpdateStateByProgress()
    {
        var progress = _growthProgress.Value;
        var newState = progress switch
        {
            < 0.25f => PlantState.Seed,
            < 0.5f => PlantState.Sprout,
            < 0.75f => PlantState.Growing,
            < 1f => PlantState.Mature,
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
            _view.UpdateSprite(Data.GrowthStages[stageIndex]);
        }

        // Обновляем цвет в зависимости от состояния
        if (_state.Value == PlantState.Withered)
        {
            _view.SetWitheredVisual();
        }
    }

    private int GetSpriteIndexForState(PlantState state)
    {
        return state switch
        {
            PlantState.Seed => 0,
            PlantState.Sprout => 1,
            PlantState.Growing => 2,
            PlantState.Mature => 3,
            PlantState.FullyGrown => 4,
            PlantState.Withered => 4, // Используем последний спрайт, но с эффектом
            _ => 0
        };
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

        // Проверяем шанс увядания каждые 10 секунд
        Observable.Interval(TimeSpan.FromSeconds(10))
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
        // Базовый шанс увядания зависит от редкости
        _witherChance = Data.Rarity switch
        {
            PlantRarity.Common => 0.05f,      // 5% за проверку
            PlantRarity.Uncommon => 0.07f,    // 7%
            PlantRarity.Rare => 0.1f,         // 10%
            PlantRarity.Epic => 0.12f,        // 12%
            PlantRarity.Legendary => 0.15f,   // 15% - высокий риск за высокую награду
            _ => 0.05f
        };
    }

    private int CalculateCoinsReward()
    {
        var baseReward = Data.SellPrice;
        var rarityMultiplier = GetRarityMultiplier();
        return Mathf.RoundToInt(baseReward * rarityMultiplier);
    }

    private PetalData CalculatePetalsReward()
    {
        var basePetals = GetBasePetalsAmount();
        return new PetalData(Data.Type, basePetals);
    }

    private int GetBasePetalsAmount()
    {
        return Data.Rarity switch
        {
            PlantRarity.Common => 1,
            PlantRarity.Uncommon => 2,
            PlantRarity.Rare => 3,
            PlantRarity.Epic => 5,
            PlantRarity.Legendary => 10,
            _ => 1
        };
    }

    private float GetRarityMultiplier()
    {
        return Data.Rarity switch
        {
            PlantRarity.Common => 1f,
            PlantRarity.Uncommon => 1.5f,
            PlantRarity.Rare => 2f,
            PlantRarity.Epic => 3f,
            PlantRarity.Legendary => 5f,
            _ => 1f
        };
    }

    private PlantHarvestResult.BonusItem[] GenerateBonusItems()
    {
        // Шанс на бонусные предметы зависит от редкости
        var bonusChance = GetBonusItemChance();
        if (UnityEngine.Random.Range(0f, 1f) > bonusChance)
        {
            return Array.Empty<PlantHarvestResult.BonusItem>();
        }

        // Генерируем случайный бонус (будет расширено позже)
        return new[] { new PlantHarvestResult.BonusItem { BonusItemType = PlantHarvestResult.BonusItem.Type.Seed, Amount = 1 } };
    }

    private float GetBonusItemChance()
    {
        return Data.Rarity switch
        {
            PlantRarity.Common => 0.1f,
            PlantRarity.Uncommon => 0.2f,
            PlantRarity.Rare => 0.3f,
            PlantRarity.Epic => 0.5f,
            PlantRarity.Legendary => 0.8f,
            _ => 0.1f
        };
    }

    private void PlayHarvestAnimation()
    {
        if (_view != null)
        {
            _view.PlayHarvestAnimation();
        }
    }

    public void StopGrowing()
    {
        // Отписываемся от обновлений роста
        _disposables.Clear();
    }
}