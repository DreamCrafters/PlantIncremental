using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using VContainer;

/// <summary>
/// Реализация менеджера полива растений
/// </summary>
public class WateringManager : IWateringManager
{
    private readonly ITimeService _timeService;
    private readonly GameSettings _gameSettings;
    
    private readonly Dictionary<IPlantEntity, float> _lastWateringTimes = new();
    private readonly Dictionary<IPlantEntity, IDisposable> _witherTimers = new();
    private readonly Dictionary<IPlantEntity, IDisposable> _growthTimers = new();
    
    private readonly Subject<IPlantEntity> _onPlantWatered = new();
    private readonly Subject<IPlantEntity> _onPlantWithered = new();
    private readonly CompositeDisposable _disposables = new();

    public IObservable<IPlantEntity> OnPlantWatered => _onPlantWatered;
    public IObservable<IPlantEntity> OnPlantWithered => _onPlantWithered;

    [Inject]
    public WateringManager(ITimeService timeService, GameSettings gameSettings)
    {
        _timeService = timeService ?? throw new ArgumentNullException(nameof(timeService));
        _gameSettings = gameSettings ?? throw new ArgumentNullException(nameof(gameSettings));
    }

    public bool WaterPlant(IPlantEntity plant)
    {
        if (plant == null)
        {
            Debug.LogWarning("Cannot water null plant");
            return false;
        }

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
                StartGrowthTimer(plant, 1);
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

    public void StartWitherTimer(IPlantEntity plant)
    {
        if (plant == null) return;

        // Останавливаем предыдущий таймер
        StopWitherTimer(plant);

        var witherTimer = _timeService.CreateTimer(TimeSpan.FromSeconds(_gameSettings.WitheringDuration))
            .Subscribe(_ =>
            {
                if (NeedsWatering(plant))
                {
                    // Растение увядает
                    plant.UpdateState(PlantState.Withered);
                    plant.SetWaitingForWater(false); // Увядшее растение больше не ждет полива
                    
                    _onPlantWithered.OnNext(plant);
                    _witherTimers.Remove(plant);
                }
            })
            .AddTo(_disposables);

        _witherTimers[plant] = witherTimer;
    }

    public void StopWitherTimer(IPlantEntity plant)
    {
        if (plant == null) return;

        if (_witherTimers.TryGetValue(plant, out var timer))
        {
            timer?.Dispose();
            _witherTimers.Remove(plant);
        }
    }

    public bool NeedsWatering(IPlantEntity plant)
    {
        if (plant == null) return false;
        
        // Растение нуждается в поливе если:
        // 1. Оно ожидает полива
        // 2. Не увяло
        // 3. Не полностью выросло
        return plant.IsWaitingForWater && 
               plant.State.Value != PlantState.Withered && 
               plant.State.Value != PlantState.FullyGrown;
    }

    public float GetTimeSinceLastWatering(IPlantEntity plant)
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
    private void StartGrowthTimer(IPlantEntity plant, float growthModifier)
    {
        if (plant == null) return;

        // Останавливаем предыдущий таймер роста
        StopGrowthTimer(plant);

        var growthTimer = _timeService.CreateTimer(TimeSpan.FromSeconds(plant.Data.GrowthTime * growthModifier / 2))
            .Subscribe(_ =>
            {
                // Через N секунд растение требует полива для следующей стадии
                plant.SetWaitingForWater(true);
                _growthTimers.Remove(plant);
            })
            .AddTo(_disposables);

        _growthTimers[plant] = growthTimer;
    }

    /// <summary>
    /// Останавливает таймер роста для растения
    /// </summary>
    private void StopGrowthTimer(IPlantEntity plant)
    {
        if (plant == null) return;

        if (_growthTimers.TryGetValue(plant, out var timer))
        {
            timer?.Dispose();
            _growthTimers.Remove(plant);
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