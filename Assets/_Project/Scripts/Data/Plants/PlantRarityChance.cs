using UnityEngine;

[System.Serializable]
public class PlantRarityChance
{
    public PlantRarity Rarity;
    [Range(0f, 1f)] public float Chance;
}