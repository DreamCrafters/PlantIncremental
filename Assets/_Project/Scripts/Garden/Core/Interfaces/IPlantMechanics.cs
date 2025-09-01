using UnityEngine;

public interface IPlantMechanics
{
    /// <summary>
    /// Вызывается когда растение посажено
    /// </summary>
    void OnPlanted(PlantEntity plant, Vector2Int gridPosition);

    /// <summary>
    /// Вызывается когда растение поливают
    /// </summary>
    void OnWatered(PlantEntity plant);

    /// <summary>
    /// Вызывается когда растение собирают
    /// </summary>
    void OnHarvested(PlantEntity plant, PlantHarvestResult result);

    /// <summary>
    /// Вызывается при изменении стадии роста
    /// </summary>
    void OnGrowthStageChanged(PlantEntity plant, PlantState newState);
}