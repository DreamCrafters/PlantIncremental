using UnityEngine;

/// <summary>
/// События для системы сетки
/// </summary>
public struct PlantHarvestedEvent
{
    public PlantEntity Plant;
    public Vector2Int Position;
    public RewardResult Reward;
}
