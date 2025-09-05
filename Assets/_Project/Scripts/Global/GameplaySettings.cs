using UnityEngine;

[CreateAssetMenu(fileName = "GameplaySettings", menuName = "Game/Settings/Gameplay Settings")]
public class GameplaySettings : ScriptableObject
{
    [Header("Save System")]
    public float AutoSaveInterval = 30f;

    [Header("Day Cycle")]
    public float DayDuration = 180f;
}