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
        return true;
    }

    public IPlantEntity Harvest()
    {
        var plant = Plant;
        Plant = null;
        return plant;
    }
    
    /// <summary>
    /// Получает модификатор скорости роста в зависимости от типа почвы
    /// </summary>
    public float GetGrowthModifier()
    {
        return SoilType switch
        {
            SoilType.Fertile => 1.0f,
            SoilType.Base => 0.75f,
            SoilType.Rocky => 0.5f,
            _ => 1.0f
        };
    }
}