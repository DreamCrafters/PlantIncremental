using System.Collections.Generic;

/// <summary>
/// Дерево навыков
/// </summary>
public class SkillTree : ISkillTree
{
    private readonly Dictionary<string, SkillNode> _nodes = new();
    private readonly IEconomyService _economy;
    
    public string TreeId { get; }
    public string TreeName { get; }
    public IReadOnlyDictionary<string, ISkillNode> Nodes => (IReadOnlyDictionary<string, ISkillNode>)_nodes;
    
    public SkillTree(SkillTreeData data, ISkillFactory factory, IEconomyService economy)
    {
        TreeId = data.TreeId;
        TreeName = data.TreeName;
        _economy = economy;
        
        // Создаем узлы
        foreach (var nodeData in data.Nodes)
        {
            var skill = factory.CreateSkill(nodeData.Skill);
            var node = new SkillNode(nodeData.NodeId, skill, nodeData.PrerequisiteNodeIds);
            _nodes[nodeData.NodeId] = node;
        }
        
        UpdateNodeAvailability();
    }
    
    public bool TryUpgradeSkill(string nodeId)
    {
        if (!_nodes.TryGetValue(nodeId, out var node))
            return false;
        
        // Проверяем доступность узла
        if (!node.IsAvailable.Value)
            return false;
        
        // Проверяем возможность улучшения
        if (!node.Skill.CanUpgrade())
            return false;
        
        // Улучшаем навык
        node.Skill.Upgrade();
        
        // Если это первое улучшение - разблокируем узел
        if (node.Skill.CurrentLevel.Value == 1)
        {
            node.SetUnlocked(true);
            UpdateNodeAvailability();
        }
        
        return true;
    }
    
    public ISkillNode GetNode(string nodeId)
    {
        return _nodes.TryGetValue(nodeId, out var node) ? node : null;
    }
    
    public void UpdateNodeAvailability()
    {
        foreach (var node in _nodes.Values)
        {
            node.UpdateAvailability(id => 
                _nodes.TryGetValue(id, out var n) && n.IsUnlocked.Value);
        }
    }
    
    public void Reset()
    {
        foreach (var node in _nodes.Values)
        {
            node.SetUnlocked(false);
            if (node.Skill is BaseSkill baseSkill)
            {
                baseSkill.SetLevel(0);
            }
        }
        UpdateNodeAvailability();
    }
}