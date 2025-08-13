using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Конфигурация дерева навыков
/// </summary>
[CreateAssetMenu(fileName = "SkillTree", menuName = "Game/Skill Tree")]
public class SkillTreeData : ScriptableObject
{
    [Header("Tree Info")]
    public string TreeId;
    public string TreeName;
    [TextArea(2, 4)]
    public string Description;
    
    [Header("Nodes")]
    public List<SkillNodeData> Nodes = new();
    
    [Header("Visual")]
    public Sprite BackgroundImage;
    
    // Валидация в редакторе
    private void OnValidate()
    {
        // Проверяем уникальность ID
        var nodeIds = new HashSet<string>();
        foreach (var node in Nodes)
        {
            if (string.IsNullOrEmpty(node.NodeId))
            {
                Debug.LogWarning($"Empty NodeId in {name}");
                continue;
            }
            
            if (!nodeIds.Add(node.NodeId))
            {
                Debug.LogError($"Duplicate NodeId: {node.NodeId} in {name}");
            }
            
            // Проверяем что prerequisites существуют
            if (node.PrerequisiteNodeIds != null)
            {
                foreach (var prereq in node.PrerequisiteNodeIds)
                {
                    if (!string.IsNullOrEmpty(prereq) && !NodeExists(prereq))
                    {
                        Debug.LogWarning($"Prerequisite {prereq} not found for node {node.NodeId}");
                    }
                }
            }
        }
    }
    
    private bool NodeExists(string nodeId)
    {
        return Nodes.Exists(n => n.NodeId == nodeId);
    }
}