using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Grid")]
    public Vector2Int GridSize = new(6, 6);

    [Header("Save")]
    public float AutoSaveInterval = 30f;

    [Header("Day Cycle")]
    public float DayDuration = 180f;

    [Header("Plants")]
    public PlantData[] AvailablePlants;
}