using UnityEngine;

public interface IPlantMechanics
{
    /// <summary>
    /// Вызывается когда растение посажено
    /// </summary>
    void OnPlanted(IPlantEntity plant, Vector2Int gridPosition);

    /// <summary>
    /// Вызывается когда растение поливают
    /// </summary>
    void OnWatered(IPlantEntity plant);

    /// <summary>
    /// Вызывается когда растение собирают
    /// </summary>
    void OnHarvested(IPlantEntity plant, PlantHarvestResult result);

    /// <summary>
    /// Вызывается при изменении стадии роста
    /// </summary>
    void OnGrowthStageChanged(IPlantEntity plant, PlantState newState);
}