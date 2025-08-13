using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Grid")]
    public Vector2Int GridSize = new(6, 6);
    public GridDisplayType DisplayType = GridDisplayType.Orthogonal;
    public Vector2 OrthographicTileSize = new(0.5f, 0.25f);
    public Vector2 IsometricTileSize = new(0.5f, 0.25f);

    [Header("Interaction")]
    [Tooltip("Кулдаун между взаимодействиями (в секундах)")]
    public float InteractionCooldown = 0.5f;

    [Header("Camera")]
    [Tooltip("Отступ от края карты до края камеры (в мировых единицах)")]
    public float CameraMargin = 1.0f;

    [Header("Save")]
    public float AutoSaveInterval = 30f;

    [Header("Day Cycle")]
    public float DayDuration = 180f;

    [Header("Plants")]
    public PlantView ViewPrefab;
    public PlantData[] AvailablePlants;

    [Header("Plant Rarity")]
    [Tooltip("Шанс выпадения растений по редкости (от 0 до 1). В инспекторе отображаются нормализованные значения")]
    public PlantRarityChance[] RarityChances = new PlantRarityChance[]
    {
        new() { Rarity = PlantRarity.Common, Chance = 0.6f },
        new() { Rarity = PlantRarity.Uncommon, Chance = 0.25f },
        new() { Rarity = PlantRarity.Rare, Chance = 0.1f },
        new() { Rarity = PlantRarity.Epic, Chance = 0.04f },
        new() { Rarity = PlantRarity.Legendary, Chance = 0.01f }
    };

    /// <summary>
    /// Проверяет корректность настроек редкости
    /// </summary>
    public bool ValidateRarityChances()
    {
        if (RarityChances == null || RarityChances.Length == 0) return false;

        float totalChance = 0f;
        foreach (var rarityChance in RarityChances)
        {
            if (rarityChance.Chance < 0f) return false; // Отрицательные шансы недопустимы
            totalChance += rarityChance.Chance;
        }

        // Проверяем, что общая сумма больше 0 (для нормализации)
        return totalChance > 0f;
    }

    /// <summary>
    /// Получает нормализованные шансы редкости для использования в игровой логике
    /// </summary>
    public PlantRarityChance[] GetNormalizedRarityChances()
    {
        if (RarityChances == null || RarityChances.Length == 0) 
            return new PlantRarityChance[0];

        float totalChance = 0f;
        foreach (var rarityChance in RarityChances)
        {
            totalChance += rarityChance.Chance;
        }

        if (totalChance <= 0f) 
            return new PlantRarityChance[0];

        var normalizedChances = new PlantRarityChance[RarityChances.Length];
        for (int i = 0; i < RarityChances.Length; i++)
        {
            normalizedChances[i] = new PlantRarityChance
            {
                Rarity = RarityChances[i].Rarity,
                Chance = RarityChances[i].Chance / totalChance
            };
        }

        return normalizedChances;
    }

    /// <summary>
    /// Получает количество растений каждой редкости
    /// </summary>
    public System.Collections.Generic.Dictionary<PlantRarity, int> GetPlantCountByRarity()
    {
        var counts = new System.Collections.Generic.Dictionary<PlantRarity, int>();
        
        if (AvailablePlants == null) return counts;

        foreach (var plant in AvailablePlants)
        {
            if (plant != null)
            {
                if (!counts.ContainsKey(plant.Rarity))
                {
                    counts[plant.Rarity] = 0;
                }
                counts[plant.Rarity]++;
            }
        }

        return counts;
    }
}
