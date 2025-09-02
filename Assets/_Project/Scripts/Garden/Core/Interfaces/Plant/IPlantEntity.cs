using System;
using UniRx;
using UnityEngine;

public interface IPlantEntity : IDisposable
{
    PlantData Data { get; }
    IReadOnlyReactiveProperty<float> GrowthProgress { get; }
    IReadOnlyReactiveProperty<PlantState> State { get; }
    IReadOnlyReactiveProperty<bool> WaitingForWater { get; }
    PlantView View { get; }
    bool IsWaitingForWater { get; }
    bool IsHarvestable { get; }
    bool IsWithered { get; }
    Vector2 Position { get; }
    Vector2Int GridPosition { get; }

    /// <summary>
    /// Обновляет прогресс роста растения
    /// </summary>
    /// <param name="progress">Новый прогресс роста (0-1)</param>
    void UpdateGrowthProgress(float progress);
    
    /// <summary>
    /// Обновляет состояние растения
    /// </summary>
    /// <param name="newState">Новое состояние</param>
    void UpdateState(PlantState newState);
    
    /// <summary>
    /// Устанавливает флаг ожидания полива
    /// </summary>
    /// <param name="waiting">Ожидает ли растение полива</param>
    void SetWaitingForWater(bool waiting);
    
    /// <summary>
    /// Устанавливает позицию растения на сетке и активирует механики посадки
    /// </summary>
    /// <param name="gridPosition">Позиция на сетке</param>
    void SetGridPosition(Vector2Int gridPosition);
    
    /// <summary>
    /// Собирает растение и возвращает награду
    /// </summary>
    /// <returns>Результат сбора урожая</returns>
    PlantHarvestResult Harvest();

    /// <summary>
    /// Поливает растение, увеличивая его прогресс роста
    /// </summary>
    void Water();
    
    /// <summary>
    /// Активирует пассивную способность растения
    /// </summary>
    void ActivatePassiveAbility();
    
    /// <summary>
    /// Вызывает механики полива
    /// </summary>
    void TriggerWaterMechanics();
}