using System;

/// <summary>
/// Сервис для управления поливом растений
/// </summary>
public interface IWateringManager : IDisposable
{
    /// <summary>
    /// Поливает указанное растение
    /// </summary>
    /// <param name="plant">Растение для полива</param>
    /// <returns>True если полив прошел успешно</returns>
    bool WaterPlant(IPlantEntity plant);
    
    /// <summary>
    /// Останавливает таймер увядания для растения
    /// </summary>
    /// <param name="plant">Растение для остановки таймера</param>
    void StopWitherTimer(IPlantEntity plant);
    
    /// <summary>
    /// Проверяет, нуждается ли растение в поливе
    /// </summary>
    /// <param name="plant">Растение для проверки</param>
    /// <returns>True если растение нуждается в поливе</returns>
    bool NeedsWatering(IPlantEntity plant);
    
    /// <summary>
    /// Получает время с последнего полива растения
    /// </summary>
    /// <param name="plant">Растение для проверки</param>
    /// <returns>Время в секундах с последнего полива</returns>
    float GetTimeSinceLastWatering(IPlantEntity plant);
    
    /// <summary>
    /// События полива растения
    /// </summary>
    IObservable<IPlantEntity> OnPlantWatered { get; }
    
    /// <summary>
    /// События увядания растения
    /// </summary>
    IObservable<IPlantEntity> OnPlantWithered { get; }
}