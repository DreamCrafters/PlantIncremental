using UnityEngine;

[CreateAssetMenu(fileName = "SoilSettings", menuName = "Game/Settings/Soil Settings")]
public class SoilSettings : ScriptableObject
{
    [Header("Soil Configuration")]
    [Tooltip("Шанс выпадения почвы по редкости (от 0 до 1). В инспекторе отображаются нормализованные значения")]
    public SoilInfo[] SoilInfo = new SoilInfo[]
    {
        new() { Type = SoilType.Fertile, Chance = 0.6f },
        new() { Type = SoilType.Rocky, Chance = 0.3f },
        new() { Type = SoilType.Unsuitable, Chance = 0.1f },
    };

    /// <summary>
    /// Получает нормализованные шансы типов почвы для использования в игровой логике
    /// </summary>
    public SoilInfo[] GetNormalizedSoilTypeChances()
    {
        if (SoilInfo == null || SoilInfo.Length == 0)
            return new SoilInfo[0];

        float totalChance = 0f;
        foreach (var soilChance in SoilInfo)
        {
            totalChance += soilChance.Chance;
        }

        if (totalChance <= 0f)
            return new SoilInfo[0];

        var normalizedChances = new SoilInfo[SoilInfo.Length];
        for (int i = 0; i < SoilInfo.Length; i++)
        {
            normalizedChances[i] = new SoilInfo
            {
                Type = SoilInfo[i].Type,
                Chance = SoilInfo[i].Chance / totalChance
            };
        }

        return normalizedChances;
    }
}