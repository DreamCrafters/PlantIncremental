using System.Collections.Generic;
using UniRx;

/// <summary>
/// Узел в дереве навыков
/// </summary>
public interface ISkillNode
{
    string NodeId { get; }
    ISkill Skill { get; }
    IReadOnlyList<string> Prerequisites { get; } // ID других нод
    IReadOnlyReactiveProperty<bool> IsUnlocked { get; }
    IReadOnlyReactiveProperty<bool> IsAvailable { get; } // Доступен для прокачки
}