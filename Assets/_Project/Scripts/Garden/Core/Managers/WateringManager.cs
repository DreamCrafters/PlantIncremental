using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using VContainer;

/// <summary>
/// Реализация менеджера полива растений
/// </summary>
public class WateringManager
{
    private readonly TimeService _timeService;
    private readonly GameSettings _gameSettings;
    private readonly GridService _gridService;
    private readonly InputService _inputService;

    private readonly Dictionary<PlantEntity, float> _lastWateringTimes = new();
    private readonly Dictionary<PlantEntity, IDisposable> _witherTimers = new();
    private readonly Dictionary<PlantEntity, IDisposable> _growthTimers = new();

    private readonly Subject<PlantEntity> _onPlantWatered = new();
    private readonly Subject<PlantEntity> _onPlantWithered = new();
    private readonly CompositeDisposable _disposables = new();

    public IObservable<PlantEntity> OnPlantWatered => _onPlantWatered;
    public IObservable<PlantEntity> OnPlantWithered => _onPlantWithered;

    [Inject]
    public WateringManager(TimeService timeService, GameSettings gameSettings, GridService gridService, InputService inputService)
    {
        _timeService = timeService;
        _gameSettings = gameSettings;
        _gridService = gridService;
        _inputService = inputService;
    }

    public bool WaterPlant(PlantEntity plant)
    {
        if (plant == null)
        {
            Debug.LogWarning("Cannot water null plant");
            return false;
        }

        StartWitherTimer(plant);

        if (!NeedsWatering(plant))
        {
            return false;
        }

        // Записываем время полива
        _lastWateringTimes[plant] = _timeService.CurrentTime;

        // НОВАЯ ЛОГИКА: Мгновенный переход к следующей стадии при поливе
        var currentStage = plant.State.Value;
        var nextStage = GetNextGrowthStage(currentStage);
        if (nextStage.HasValue)
        {
            // Мгновенно переводим в следующую стадию
            plant.UpdateState(nextStage.Value);
            plant.UpdateGrowthProgress(GetProgressForStage(nextStage.Value));

            // Если это не финальная стадия - запускаем таймер для следующего полива
            if (nextStage.Value != PlantState.FullyGrown)
            {
                var growthModifier = GetGrowthModifierForPlant(plant);
                StartGrowthTimer(plant, growthModifier);
            }

        }

        // Убираем флаг ожидания полива
        plant.SetWaitingForWater(false);

        // Вызываем механики полива
        plant.TriggerWaterMechanics();

        // Останавливаем таймер увядания
        StopWitherTimer(plant);

        // Уведомляем о поливе
        _onPlantWatered.OnNext(plant);

        return true;
    }

    private void StartWitherTimer(PlantEntity plant)
    {
        if (plant == null) return;

        // Останавливаем предыдущий таймер
        StopWitherTimer(plant);

        var witherTimer = _timeService.CreateTimer(TimeSpan.FromSeconds(plant.Data.WitherTime))
            .Subscribe(_ =>
            {
                // Растение увядает
                plant.UpdateState(PlantState.Withered);
                plant.SetWaitingForWater(false); // Увядшее растение больше не ждет полива

                _onPlantWithered.OnNext(plant);
                _witherTimers.Remove(plant);
            })
            .AddTo(_disposables);

        _witherTimers[plant] = witherTimer;
    }

    public void StopWitherTimer(PlantEntity plant)
    {
        if (plant == null) return;

        if (_witherTimers.TryGetValue(plant, out var timer))
        {
            timer?.Dispose();
            _witherTimers.Remove(plant);
        }
    }

    public bool NeedsWatering(PlantEntity plant)
    {
        if (plant == null) return false;
        
        return plant.IsWaitingForWater && 
               plant.State.Value != PlantState.Withered && 
               plant.State.Value != PlantState.FullyGrown;
    }

    public float GetTimeSinceLastWatering(PlantEntity plant)
    {
        if (plant == null) return 0f;

        if (_lastWateringTimes.TryGetValue(plant, out var lastTime))
        {
            return _timeService.CurrentTime - lastTime;
        }

        return 0f;
    }

    /// <summary>
    /// Определяет следующую стадию роста для текущего состояния растения
    /// </summary>
    private PlantState? GetNextGrowthStage(PlantState currentState)
    {
        return currentState switch
        {
            PlantState.New => PlantState.Seed,
            PlantState.Seed => PlantState.Growing,
            PlantState.Growing => PlantState.FullyGrown,
            PlantState.FullyGrown => null, // Уже финальная стадия
            PlantState.Withered => null,   // Увядшие растения не растут
            _ => null
        };
    }

    /// <summary>
    /// Возвращает прогресс роста для указанной стадии
    /// </summary>
    private float GetProgressForStage(PlantState stage)
    {
        return stage switch
        {
            PlantState.New => 0f,
            PlantState.Seed => 0f,
            PlantState.Growing => 0.5f,
            PlantState.FullyGrown => 1f,
            PlantState.Withered => 0f,
            _ => 0f
        };
    }

    /// <summary>
    /// Запускает таймер роста для следующего полива
    /// </summary>
    private void StartGrowthTimer(PlantEntity plant, float growthModifier)
    {
        if (plant == null) return;

        // Останавливаем предыдущий таймер роста
        StopGrowthTimer(plant);

        // Защита от деления на ноль и переполнения TimeSpan
        const float minModifier = 0.01f;
        const float maxDurationSeconds = 86400f; // 24 часа максимум

        growthModifier = Mathf.Max(growthModifier, minModifier);
        float durationSeconds = plant.Data.GrowthTimePerStage / growthModifier;
        durationSeconds = Mathf.Min(durationSeconds, maxDurationSeconds);

        if (durationSeconds <= 0 || float.IsInfinity(durationSeconds) || float.IsNaN(durationSeconds))
        {
            Debug.LogWarning($"Invalid growth duration calculated for plant {plant.Data.name}: {durationSeconds}. Using default 10 seconds.");
            durationSeconds = 10f;
        }

        var growthTimer = _timeService.CreateTimer(TimeSpan.FromSeconds(durationSeconds))
            .Subscribe(_ =>
            {
                // Через N секунд растение требует полива для следующей стадии
                plant.SetWaitingForWater(true);
                _inputService.RestartCellButtonTimer(plant);
                _growthTimers.Remove(plant);
            })
            .AddTo(_disposables);

        _growthTimers[plant] = growthTimer;
    }

    /// <summary>
    /// Останавливает таймер роста для растения
    /// </summary>
    private void StopGrowthTimer(PlantEntity plant)
    {
        if (plant == null) return;

        if (_growthTimers.TryGetValue(plant, out var timer))
        {
            timer?.Dispose();
            _growthTimers.Remove(plant);
        }
    }

    /// <summary>
    /// Получает модификатор роста для растения на основе типа почвы
    /// </summary>
    private float GetGrowthModifierForPlant(PlantEntity plant)
    {
        if (plant == null) return 1f;

        try
        {
            var gridCell = _gridService.GetCell(plant.GridPosition);
            return gridCell?.GetGrowthModifier(_gameSettings) ?? 1f;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting growth modifier for plant: {ex.Message}");
            return 1f;
        }
    }

    public void Dispose()
    {
        // Останавливаем все таймеры увядания
        foreach (var timer in _witherTimers.Values)
        {
            timer?.Dispose();
        }
        _witherTimers.Clear();

        // Останавливаем все таймеры роста
        foreach (var timer in _growthTimers.Values)
        {
            timer?.Dispose();
        }
        _growthTimers.Clear();

        _lastWateringTimes.Clear();

        _onPlantWatered?.Dispose();
        _onPlantWithered?.Dispose();
        _disposables?.Dispose();
    }
}