using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using VContainer;

/// <summary>
/// Реализация менеджера роста растений
/// </summary>
public class PlantGrowthManager : IPlantGrowthManager
{
    private readonly Dictionary<IPlantEntity, IDisposable> _growthSubscriptions = new();
    private readonly CompositeDisposable _disposables = new();

    [Inject]
    public PlantGrowthManager()
    {
    }

    public void StartGrowth(IPlantEntity plant, float growthModifier = 1f)
    {
        if (plant == null)
        {
            Debug.LogWarning("Cannot start growth for null plant");
            return;
        }

        // НОВАЯ ЛОГИКА: Рост теперь управляется WateringManager через мгновенные переходы
        // Этот метод оставлен для совместимости, но не выполняет постепенный рост
        
        // Если растение уже полностью выросло или увяло, ничего не делаем
        if (plant.State.Value == PlantState.Withered || plant.State.Value == PlantState.FullyGrown)
        {
            return;
        }

        // Убираем из старой системы отслеживания роста, если было
        StopGrowth(plant);
    }

    public void StopGrowth(IPlantEntity plant)
    {
        if (plant == null) return;

        if (_growthSubscriptions.TryGetValue(plant, out var subscription))
        {
            subscription?.Dispose();
            _growthSubscriptions.Remove(plant);
        }
    }

    public void StopAllGrowth()
    {
        foreach (var subscription in _growthSubscriptions.Values)
        {
            subscription?.Dispose();
        }
        _growthSubscriptions.Clear();
    }

    public bool IsGrowing(IPlantEntity plant)
    {
        if (plant == null) return false;
        return _growthSubscriptions.ContainsKey(plant);
    }

    public void Dispose()
    {
        StopAllGrowth();
        _disposables?.Dispose();
    }
}