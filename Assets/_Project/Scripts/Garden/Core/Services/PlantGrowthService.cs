using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using VContainer;

/// <summary>
/// Сервис управления ростом всех растений в игре
/// </summary>
public class PlantGrowthService : IPlantGrowthService, IDisposable
{
    private readonly Dictionary<IPlantEntity, IDisposable> _growingPlants = new();
    private readonly Subject<IPlantEntity> _onPlantGrown = new();
    private readonly CompositeDisposable _disposables = new();
    
    private readonly IGridService _gridService;
    
    // Кэш для оптимизации
    private readonly Dictionary<IPlantEntity, float> _growthModifiers = new();
    
    public IObservable<IPlantEntity> OnPlantGrown => _onPlantGrown;
    
    [Inject]
    public PlantGrowthService(IGridService gridService)
    {
        _gridService = gridService;
    }
    
    /// <summary>
    /// Начинает процесс роста для растения
    /// </summary>
    public void StartGrowing(IPlantEntity plant)
    {
        if (plant == null || _growingPlants.ContainsKey(plant)) return;

        if (plant is not PlantEntity entity) return;

        // Получаем начальный модификатор роста
        var initialModifier = CalculateGrowthModifier(plant);
        _growthModifiers[plant] = initialModifier;
        
        // Запускаем рост
        entity.StartGrowing(initialModifier);
        
        // Подписываемся на изменения состояния
        var subscription = plant.State
            .Where(state => state == PlantState.FullyGrown || state == PlantState.Withered)
            .Take(1)
            .Subscribe(state =>
            {
                if (state == PlantState.FullyGrown)
                {
                    OnPlantFullyGrown(plant);
                }
                else if (state == PlantState.Withered)
                {
                    OnPlantWithered(plant);
                }
            });
        
        _growingPlants[plant] = subscription;
    }
    
    /// <summary>
    /// Останавливает рост растения
    /// </summary>
    public void StopGrowing(IPlantEntity plant)
    {
        if (plant == null || !_growingPlants.ContainsKey(plant)) return;
        
        // Отписываемся от обновлений
        _growingPlants[plant]?.Dispose();
        _growingPlants.Remove(plant);
        _growthModifiers.Remove(plant);
        
        // Останавливаем рост в самой сущности
        if (plant is PlantEntity entity)
        {
            entity.StopGrowing();
        }
    }
    
    /// <summary>
    /// Возвращает статистику роста
    /// </summary>
    public GrowthStatistics GetStatistics()
    {
        return new GrowthStatistics
        {
            TotalGrowingPlants = _growingPlants.Count,
            AverageGrowthModifier = _growthModifiers.Any() ? _growthModifiers.Values.Average() : 1f,
            FastestGrowingPlant = GetFastestGrowingPlant(),
            SlowestGrowingPlant = GetSlowestGrowingPlant()
        };
    }

    public void Dispose()
    {
        // Останавливаем рост всех растений
        foreach (var plant in _growingPlants.Keys.ToList())
        {
            StopGrowing(plant);
        }
        
        _growingPlants.Clear();
        _growthModifiers.Clear();
        _onPlantGrown?.Dispose();
        _disposables?.Dispose();
    }

    /// <summary>
    /// Рассчитывает модификатор скорости роста на основе окружения
    /// </summary>
    private float CalculateGrowthModifier(IPlantEntity plant)
    {
        var baseModifier = 1f;
        
        // Находим клетку с растением
        var cell = FindCellWithPlant(plant);
        if (cell == null) return baseModifier;
        
        // Модификатор от типа почвы
        var soilModifier = cell.GetGrowthModifier();
        
        // Модификаторы от соседних растений
        var neighborModifier = CalculateNeighborModifiers(cell.Position);
        
        // Глобальные модификаторы (будут добавлены позже через систему навыков)
        var globalModifier = GetGlobalGrowthModifier();
        
        return baseModifier * soilModifier * neighborModifier * globalModifier;
    }
    
    /// <summary>
    /// Рассчитывает модификаторы от соседних растений
    /// </summary>
    private float CalculateNeighborModifiers(Vector2Int position)
    {
        var modifier = 1f;
        var neighbors = _gridService.GetNeighbors(position);
        
        foreach (var neighbor in neighbors)
        {
            if (neighbor.IsEmpty || neighbor.Plant.State.Value != PlantState.FullyGrown) 
                continue;
            
            // Проверяем тип растения и его пассивную способность
            var plantType = neighbor.Plant.Data.Type;
            modifier *= GetPlantTypeGrowthBonus(plantType);
        }
        
        return modifier;
    }
    
    /// <summary>
    /// Возвращает бонус к росту от типа растения
    /// </summary>
    private float GetPlantTypeGrowthBonus(PlantType type)
    {
        // Это будет расширено когда добавим больше типов растений
        // Пока что базовая логика
        return type switch
        {
            PlantType.Basic => 1.1f, // Базовое растение даёт 10% бонус
            _ => 1f
        };
    }
    
    /// <summary>
    /// Находит клетку сетки, содержащую указанное растение
    /// </summary>
    private GridCell FindCellWithPlant(IPlantEntity plant)
    {
        var grid = _gridService.Grid.Value;
        if (grid == null) return null;
        
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                if (grid[x, y].Plant == plant)
                {
                    return grid[x, y];
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Обработка полностью выросшего растения
    /// </summary>
    private void OnPlantFullyGrown(IPlantEntity plant)
    {
        _onPlantGrown.OnNext(plant);
        
        // Активируем пассивную способность
        if (plant is PlantEntity entity)
        {
            entity.ActivatePassiveAbility();
        }
    }
    
    /// <summary>
    /// Обработка увядшего растения
    /// </summary>
    private void OnPlantWithered(IPlantEntity plant)
    {
        StopGrowing(plant);
    }
    
    /// <summary>
    /// Получает глобальный модификатор роста (из системы навыков)
    /// </summary>
    private float GetGlobalGrowthModifier()
    {
        // TODO: Интеграция с системой навыков
        // Пока возвращаем базовое значение
        return 1f;
    }
    
    private IPlantEntity GetFastestGrowingPlant()
    {
        if (_growthModifiers.Any() == false) return null;
        
        var fastest = _growthModifiers.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
        return fastest.Key;
    }
    
    private IPlantEntity GetSlowestGrowingPlant()
    {
        if (_growthModifiers.Any() == false) return null;
        
        var slowest = _growthModifiers.OrderBy(kvp => kvp.Value).FirstOrDefault();
        return slowest.Key;
    }
}