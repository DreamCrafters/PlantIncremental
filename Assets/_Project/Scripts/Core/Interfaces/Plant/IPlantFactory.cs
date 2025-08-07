using UnityEngine;

public interface IPlantFactory
{
    IPlantEntity CreatePlant(PlantData data, Vector2 position);
}
