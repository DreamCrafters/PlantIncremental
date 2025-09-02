using UnityEngine;

[System.Serializable]
public class GridCell
{
    public Vector2Int Position { get; }
    public SoilType SoilType { get; set; }
    public IPlantEntity Plant { get; private set; }

    public bool IsEmpty => Plant == null;

    public GridCell(Vector2Int position, SoilType soilType)
    {
        Position = position;
        SoilType = soilType;
        Plant = null;
    }

    public bool TryPlant(IPlantEntity plant)
    {
        if (!IsEmpty) return false;
        Plant = plant;
        
        // Активируем механики посадки растения
        if (plant is PlantEntity plantEntity)
        {
            plantEntity.SetGridPosition(Position);
        }
        
        return true;
    }

    public IPlantEntity Harvest()
    {
        var plant = Plant;
        Plant = null;
        return plant;
    }
}