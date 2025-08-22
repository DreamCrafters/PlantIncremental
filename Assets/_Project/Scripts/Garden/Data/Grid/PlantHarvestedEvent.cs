using UnityEngine;

/// <summary>
/// События для системы сетки
/// </summary>
public struct PlantHarvestedEvent
{
    public IPlantEntity Plant;
    public Vector2Int Position;
    public RewardResult Reward;
}
