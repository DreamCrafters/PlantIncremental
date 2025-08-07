using UnityEngine;

public class GridCell
{
    public Vector2Int Position { get; }
    public SoilType SoilType { get; set; }
    public IPlantEntity Plant { get; private set; }

    public bool IsEmpty => Plant == null;

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
}