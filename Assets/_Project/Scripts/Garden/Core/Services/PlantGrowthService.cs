using System;
using UniRx;
using UnityEngine;
using VContainer;

/// <summary>
/// Сервис управления ростом всех растений в игре (обертка над PlantGrowthManager для совместимости)
/// </summary>
public class PlantGrowthService : IPlantGrowthService, IDisposable
{
    private readonly IPlantGrowthManager _growthManager;
    private readonly IWateringManager _wateringManager;
    private readonly Subject<IPlantEntity> _onPlantGrown = new();
    private readonly CompositeDisposable _disposables = new();
    
    public IObservable<IPlantEntity> OnPlantGrown => _onPlantGrown;
    
    [Inject]
    public PlantGrowthService(IPlantGrowthManager growthManager, IWateringManager wateringManager)
    {
        _growthManager = growthManager ?? throw new ArgumentNullException(nameof(growthManager));
        _wateringManager = wateringManager ?? throw new ArgumentNullException(nameof(wateringManager));
        
        // Подписываемся на события полива для автоматического запуска роста
        _wateringManager.OnPlantWatered
            .Subscribe(plant => _growthManager.StartGrowth(plant))
            .AddTo(_disposables);
    }
    
    public void Dispose()
    {
        _onPlantGrown?.Dispose();
        _disposables?.Dispose();
    }

    /// <summary>
    /// Начинает процесс роста для растения (перенаправляет к PlantGrowthManager)
    /// </summary>
    public void StartGrowing(IPlantEntity plant)
    {
        if (plant == null)
        {
            Debug.LogWarning("Cannot start growing null plant");
            return;
        }

        // Перенаправляем к новому менеджеру роста
        _growthManager.StartGrowth(plant);
        
        // Подписываемся на изменения состояния для уведомлений о полном росте
        plant.State
            .Where(state => state == PlantState.FullyGrown)
            .Take(1)
            .Subscribe(state => _onPlantGrown.OnNext(plant))
            .AddTo(_disposables);
    }
    
    /// <summary>
    /// Останавливает рост растения (перенаправляет к PlantGrowthManager)
    /// </summary>
    public void StopGrowing(IPlantEntity plant)
    {
        if (plant == null)
        {
            Debug.LogWarning("Cannot stop growing null plant");
            return;
        }
        
        // Перенаправляем к новому менеджеру роста
        _growthManager.StopGrowth(plant);
    }
    
    /// <summary>
    /// Возвращает статистику роста (упрощенная версия для совместимости)
    /// </summary>
    public GrowthStatistics GetStatistics()
    {
        return new GrowthStatistics
        {
            TotalGrowingPlants = 0, // Статистика теперь в PlantGrowthManager
            AverageGrowthModifier = 1f,
            FastestGrowingPlant = null,
            SlowestGrowingPlant = null
        };
    }
}