using UnityEngine;

[System.Serializable]
public class SoilTypeChance
{
    public SoilType Type;
    [Range(0f, 1f)] public float Chance;
}