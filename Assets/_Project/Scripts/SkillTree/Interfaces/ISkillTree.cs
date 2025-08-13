using System.Collections.Generic;

/// <summary>
/// Дерево навыков
/// </summary>
public interface ISkillTree
{
    string TreeId { get; }
    string TreeName { get; }
    IReadOnlyDictionary<string, ISkillNode> Nodes { get; }
    
    bool TryUpgradeSkill(string nodeId);
    ISkillNode GetNode(string nodeId);
}