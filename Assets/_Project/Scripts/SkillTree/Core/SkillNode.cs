using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

/// <summary>
/// Узел дерева навыков
/// </summary>
public class SkillNode : ISkillNode
{
    private readonly ReactiveProperty<bool> _isUnlocked = new(false);
    private readonly ReactiveProperty<bool> _isAvailable = new(false);
    
    public string NodeId { get; }
    public ISkill Skill { get; }
    public IReadOnlyList<string> Prerequisites { get; }
    public IReadOnlyReactiveProperty<bool> IsUnlocked => _isUnlocked;
    public IReadOnlyReactiveProperty<bool> IsAvailable => _isAvailable;
    
    public SkillNode(string nodeId, ISkill skill, string[] prerequisites)
    {
        NodeId = nodeId;
        Skill = skill;
        Prerequisites = prerequisites ?? Array.Empty<string>();
    }
    
    public void UpdateAvailability(Func<string, bool> isNodeUnlocked)
    {
        // Доступен если все пререквизиты разблокированы
        bool available = Prerequisites.All(isNodeUnlocked);
        _isAvailable.Value = available;
    }
    
    public void SetUnlocked(bool unlocked)
    {
        _isUnlocked.Value = unlocked;
    }
}