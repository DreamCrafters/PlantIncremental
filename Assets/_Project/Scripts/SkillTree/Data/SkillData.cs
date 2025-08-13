using UnityEngine;

/// <summary>
/// Данные одного навыка
/// </summary>
[System.Serializable]
public class SkillData
{
    public string Id;
    public string Name;
    [TextArea(2, 4)]
    public string Description;
    public Sprite Icon;
    public SkillType Type;
    public int MaxLevel = 5;
    
    [Header("Costs")]
    public SkillLevelCost[] LevelCosts;
    
    [Header("Effects")]
    public SkillEffect[] Effects;
    
    [System.Serializable]
    public struct SkillLevelCost
    {
        public int Level;
        public int CoinCost;
        public PlantType RequiredPetalType;
        public int PetalCost;
    }
    
    [System.Serializable]
    public struct SkillEffect
    {
        public int Level;
        public string Parameter; // Например, "growth_speed"
        public float Value;
        public bool IsMultiplier; // true = множитель, false = добавление
    }
}