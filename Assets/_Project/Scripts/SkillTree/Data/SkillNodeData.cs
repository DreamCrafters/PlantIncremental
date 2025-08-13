using UnityEngine;

/// <summary>
/// Узел дерева навыков
/// </summary>
[System.Serializable]
public class SkillNodeData
{
    public string NodeId;
    public Vector2 Position; // Позиция в UI
    public SkillData Skill;
    public string[] PrerequisiteNodeIds;
}