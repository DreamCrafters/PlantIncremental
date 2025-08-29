using System;
using UniRx;

/// <summary>
/// Интерфейс системы полива растений
/// </summary>
public interface IWateringSystem
{
    /// <summary>
    /// Начинает полив растения (долгое нажатие)
    /// </summary>
    /// <param name="plant">Растение для полива</param>
    void StartWatering(IPlantEntity plant);
    
    /// <summary>
    /// Прекращает полив растения
    /// </summary>
    /// <param name="plant">Растение</param>
    void StopWatering(IPlantEntity plant);
    
    /// <summary>
    /// Проверяет, можно ли поливать данное растение
    /// </summary>
    /// <param name="plant">Растение</param>
    /// <returns>True, если можно поливать</returns>
    bool CanWater(IPlantEntity plant);
    
    /// <summary>
    /// Событие завершения полива растения
    /// </summary>
    IObservable<IPlantEntity> OnPlantWatered { get; }
}
