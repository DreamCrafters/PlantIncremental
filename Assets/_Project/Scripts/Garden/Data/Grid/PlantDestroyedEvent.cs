using UnityEngine;

/// <summary>
/// Событие уничтожения увядшего растения
/// </summary>
public struct PlantDestroyedEvent
{
    public IPlantEntity Plant;
    public Vector2Int Position;
}
