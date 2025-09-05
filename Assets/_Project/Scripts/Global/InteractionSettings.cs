using UnityEngine;

[CreateAssetMenu(fileName = "InteractionSettings", menuName = "Game/Settings/Interaction Settings")]
public class InteractionSettings : ScriptableObject
{
    [Header("Interaction Timings")]
    [Tooltip("Кулдаун между взаимодействиями (в секундах)")]
    [Min(0)] 
    public float InteractionCooldown = 0.5f;
    
    [Min(0)] 
    public float WateringDuration = 0.5f;
}