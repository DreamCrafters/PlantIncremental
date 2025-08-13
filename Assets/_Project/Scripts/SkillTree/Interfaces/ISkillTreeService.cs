using System;
using System.Collections.Generic;

/// <summary>
/// Сервис управления деревьями навыков
/// </summary>
public interface ISkillTreeService
{
    IReadOnlyDictionary<string, ISkillTree> Trees { get; }
    IObservable<SkillUpgradedEvent> OnSkillUpgraded { get; }
    
    bool TryUpgradeSkill(string treeId, string nodeId);
    void ResetTree(string treeId);
    
    SkillTreeSaveData GetSaveData();
    void LoadSaveData(SkillTreeSaveData data);
}