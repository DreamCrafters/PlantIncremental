using UnityEngine;

[System.Serializable]
public class SoilInfo
{
    public SoilType Type;
    public float GrowingSpeed;
    [Range(0f, 1f)] public float Chance;
}