using UnityEngine;

/// <summary>
/// Механика подсолнуха: ускоряет рост соседних растений при посадке
/// </summary>
[CreateAssetMenu(fileName = "SunflowerBoostNeighborsOnPlanted", menuName = "Game/Plant Mechanics/Planted/Sunflower Boost Neighbors")]
public class SunflowerBoostNeighborsOnPlanted : OnPlantedMechanics
{
    [Header("Growth Boost Settings")]
    [SerializeField] private int boostRadius = 2;
    [SerializeField] private float growthSpeedMultiplier = 1.5f;
    [SerializeField] private bool showDebugLog = true;

    public override void Execute(PlantEntity plant, Vector2Int gridPosition)
    {
        if (showDebugLog)
        {
            Debug.Log($"🌻 Подсолнух посажен в позиции {gridPosition}! Ускоряет рост соседних растений...");
        }

        BoostNearbyPlants(plant, gridPosition);
    }

    private void BoostNearbyPlants(PlantEntity sunflower, Vector2Int position)
    {
        // TODO: Реализовать логику ускорения роста соседних растений
        // Потребуется доступ к GridService для поиска соседних растений
        // Пока что только демонстрационное сообщение

        if (showDebugLog)
        {
            Debug.Log($"☀️ Подсолнух освещает растения в радиусе {boostRadius} клеток! " +
                     $"Ускорение роста: x{growthSpeedMultiplier}");
        }
    }
}