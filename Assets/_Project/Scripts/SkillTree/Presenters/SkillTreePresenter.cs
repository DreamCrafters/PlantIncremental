using System;
using UniRx;
using UnityEngine;
using VContainer;

/// <summary>
/// Презентер для UI дерева навыков
/// </summary>
public class SkillTreePresenter : IDisposable
{
    private readonly ISkillTreeService _skillTreeService;
    private readonly CompositeDisposable _disposables = new();
    
    [Inject]
    public SkillTreePresenter(ISkillTreeService skillTreeService)
    {
        _skillTreeService = skillTreeService;
        
        // Подписываемся на события улучшения навыков
        _skillTreeService.OnSkillUpgraded
            .Subscribe(evt =>
            {
                Debug.Log($"Skill {evt.SkillId} upgraded to level {evt.NewLevel}");
                // TODO: Обновить UI
            })
            .AddTo(_disposables);
    }
    
    public void OnSkillNodeClicked(string treeId, string nodeId)
    {
        if (_skillTreeService.TryUpgradeSkill(treeId, nodeId))
        {
            // Воспроизвести звук и эффект
            Debug.Log($"Successfully upgraded node {nodeId}");
        }
        else
        {
            // Показать причину неудачи
            Debug.Log($"Cannot upgrade node {nodeId}");
        }
    }
    
    public void OnResetTreeClicked(string treeId)
    {
        _skillTreeService.ResetTree(treeId);
    }
    
    public void Dispose()
    {
        _disposables?.Dispose();
    }
}