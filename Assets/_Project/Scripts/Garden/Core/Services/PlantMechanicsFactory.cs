using UnityEngine;

/// <summary>
/// Реализация фабрики механик растений
/// </summary>
public class PlantMechanicsFactory : IPlantMechanicsFactory
{
    public IPlantMechanics CreateMechanics(PlantData plantData)
    {
        if (plantData == null)
        {
            Debug.LogWarning("PlantData is null, creating empty mechanics manager");
            return new PlantMechanicsManager(null, null, null, null);
        }

        return new PlantMechanicsManager(
            plantData.PlantedMechanics,
            plantData.WateredMechanics,
            plantData.HarvestedMechanics,
            plantData.GrowthStageChangedMechanics
        );
    }
}