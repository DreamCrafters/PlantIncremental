using UnityEngine;

/// <summary>
/// Событие уничтожения увядшего растения
/// </summary>
public struct PlantDestroyedEvent
{
    public PlantEntity Plant;
    public Vector2Int Position;
}
