using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings/Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Settings References")]
    public GridSettings GridSettings;
    public PlantSettings PlantSettings;
    public SoilSettings SoilSettings;
    public InteractionSettings InteractionSettings;
    public GameplaySettings GameplaySettings;

    /// <summary>
    /// Получает нормализованные шансы редкости для использования в игровой логике
    /// </summary>
    public PlantRarityChance[] GetNormalizedRarityChances()
    {
        return PlantSettings != null ? PlantSettings.GetNormalizedRarityChances() : new PlantRarityChance[0];
    }

    /// <summary>
    /// Получает нормализованные шансы типов почвы для использования в игровой логике
    /// </summary>
    public SoilInfo[] GetNormalizedSoilTypeChances()
    {
        return SoilSettings != null ? SoilSettings.GetNormalizedSoilTypeChances() : new SoilInfo[0];
    }

    /// <summary>
    /// Получает количество растений каждой редкости
    /// </summary>
    public System.Collections.Generic.Dictionary<PlantRarity, int> GetPlantCountByRarity()
    {
        return PlantSettings != null ? PlantSettings.GetPlantCountByRarity() : new System.Collections.Generic.Dictionary<PlantRarity, int>();
    }
}
