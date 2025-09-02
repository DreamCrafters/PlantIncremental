using System;

/// <summary>
/// Сервис для управления ростом растений
/// </summary>
public interface IPlantGrowthManager : IDisposable
{
    /// <summary>
    /// Начинает процесс роста для указанного растения
    /// </summary>
    /// <param name="plant">Растение для роста</param>
    /// <param name="growthModifier">Модификатор скорости роста</param>
    void StartGrowth(IPlantEntity plant, float growthModifier = 1f);
    
    /// <summary>
    /// Останавливает рост указанного растения
    /// </summary>
    /// <param name="plant">Растение для остановки роста</param>
    void StopGrowth(IPlantEntity plant);
    
    /// <summary>
    /// Останавливает рост всех растений
    /// </summary>
    void StopAllGrowth();
    
    /// <summary>
    /// Проверяет, растет ли указанное растение
    /// </summary>
    /// <param name="plant">Растение для проверки</param>
    /// <returns>True если растение растет</returns>
    bool IsGrowing(IPlantEntity plant);
}