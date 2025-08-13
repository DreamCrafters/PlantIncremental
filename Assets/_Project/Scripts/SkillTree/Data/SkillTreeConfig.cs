using UnityEngine;

/// <summary>
/// Конфигурация деревьев навыков
/// </summary>
[CreateAssetMenu(fileName = "SkillTreeConfig", menuName = "Game/Skill Tree Config")]
public class SkillTreeConfig : ScriptableObject
{
    [Header("Skill Trees")]
    public SkillTreeData[] Trees;
    
    [Header("Settings")]
    [Tooltip("Максимальная задержка активации навыка в мс")]
    public float MaxActivationDelayMs = 2f;
    
    [Tooltip("Максимальное количество узлов для оптимизации")]
    public int MaxNodesForOptimization = 1000;
    
    private void OnValidate()
    {
        if (Trees == null || Trees.Length == 0)
        {
            Debug.LogWarning("No skill trees configured!");
            return;
        }
        
        // Проверяем общее количество узлов
        int totalNodes = 0;
        foreach (var tree in Trees)
        {
            if (tree != null)
            {
                totalNodes += tree.Nodes.Count;
            }
        }
        
        if (totalNodes > MaxNodesForOptimization)
        {
            Debug.LogWarning($"Total nodes ({totalNodes}) exceeds optimization limit ({MaxNodesForOptimization})");
        }
    }
}