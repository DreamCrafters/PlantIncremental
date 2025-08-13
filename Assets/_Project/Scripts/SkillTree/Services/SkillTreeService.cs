using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using VContainer;


/// <summary>
/// Основной сервис управления деревьями навыков
/// </summary>
public class SkillTreeService : ISkillTreeService, IDisposable
{
    private readonly Dictionary<string, ISkillTree> _trees = new();
    private readonly Subject<SkillUpgradedEvent> _onSkillUpgraded = new();
    private readonly CompositeDisposable _disposables = new();

    private readonly ISkillFactory _skillFactory;
    private readonly IEconomyService _economy;
    private readonly ISaveService _saveService;

    public IReadOnlyDictionary<string, ISkillTree> Trees => _trees;
    public IObservable<SkillUpgradedEvent> OnSkillUpgraded => _onSkillUpgraded;

    [Inject]
    public SkillTreeService(
        ISkillFactory skillFactory,
        IEconomyService economy,
        ISaveService saveService,
        SkillTreeConfig config)
    {
        _skillFactory = skillFactory;
        _economy = economy;
        _saveService = saveService;

        // Загружаем деревья из конфигурации
        foreach (var treeData in config.Trees)
        {
            var tree = new SkillTree(treeData, skillFactory, economy);
            _trees[tree.TreeId] = tree;

            // Подписываемся на изменения навыков
            SubscribeToTree(tree);
        }

        // Загружаем сохраненный прогресс
        LoadProgress();

        // Автосохранение при изменениях
        _onSkillUpgraded
            .Throttle(TimeSpan.FromSeconds(2))
            .Subscribe(_ => SaveProgress())
            .AddTo(_disposables);
    }

    public bool TryUpgradeSkill(string treeId, string nodeId)
    {
        if (!_trees.TryGetValue(treeId, out var tree))
            return false;

        if (!tree.TryUpgradeSkill(nodeId))
            return false;

        var node = tree.GetNode(nodeId);
        if (node != null)
        {
            _onSkillUpgraded.OnNext(new SkillUpgradedEvent
            {
                TreeId = treeId,
                NodeId = nodeId,
                SkillId = node.Skill.Id,
                NewLevel = node.Skill.CurrentLevel.Value
            });
        }

        return true;
    }

    public void ResetTree(string treeId)
    {
        if (_trees.TryGetValue(treeId, out var tree) && tree is SkillTree skillTree)
        {
            // Возвращаем часть потраченных ресурсов (50%)
            var refund = CalculateRefund(tree);
            _economy.AddCoins(refund);

            skillTree.Reset();
            SaveProgress();
        }
    }

    public SkillTreeSaveData GetSaveData()
    {
        var saveData = new SkillTreeSaveData();

        foreach (var tree in _trees.Values)
        {
            foreach (var node in tree.Nodes.Values)
            {
                if (node.Skill.CurrentLevel.Value > 0)
                {
                    saveData.Skills.Add(new SkillTreeSaveData.SkillProgress
                    {
                        SkillId = node.Skill.Id,
                        Level = node.Skill.CurrentLevel.Value
                    });
                }
            }
        }

        return saveData;
    }

    public void LoadSaveData(SkillTreeSaveData data)
    {
        if (data == null || data.Skills == null) return;

        foreach (var progress in data.Skills)
        {
            // Находим навык по ID во всех деревьях
            foreach (var tree in _trees.Values)
            {
                foreach (var node in tree.Nodes.Values)
                {
                    if (node.Skill.Id == progress.SkillId && node.Skill is BaseSkill baseSkill)
                    {
                        baseSkill.SetLevel(progress.Level);
                        if (progress.Level > 0)
                        {
                            (node as SkillNode)?.SetUnlocked(true);
                        }
                    }
                }
            }
        }

        // Обновляем доступность узлов
        foreach (var tree in _trees.Values)
        {
            if (tree is SkillTree skillTree)
            {
                skillTree.UpdateNodeAvailability();
            }
        }
    }

    private void SubscribeToTree(ISkillTree tree)
    {
        foreach (var node in tree.Nodes.Values)
        {
            node.Skill.CurrentLevel
                .Skip(1)
                .Subscribe(level =>
                {
                    Debug.Log($"Skill {node.Skill.Name} upgraded to level {level}");
                })
                .AddTo(_disposables);
        }
    }

    private int CalculateRefund(ISkillTree tree)
    {
        int totalSpent = 0;
        foreach (var node in tree.Nodes.Values)
        {
            for (int i = 1; i <= node.Skill.CurrentLevel.Value; i++)
            {
                var cost = node.Skill.GetUpgradeCost(i);
                totalSpent += cost.Coins;
            }
        }
        return totalSpent / 2; // Возвращаем 50%
    }

    private void SaveProgress()
    {
        // Интеграция с существующей системой сохранений
        var saveData = GetSaveData();
        // TODO: Добавить в SaveData поле для навыков
        Debug.Log($"Saving {saveData.Skills.Count} skill progressions");
    }

    private void LoadProgress()
    {
        // TODO: Загрузка из SaveService
        Debug.Log("Loading skill tree progress...");
    }

    public void Dispose()
    {
        _disposables?.Dispose();
        _onSkillUpgraded?.Dispose();
    }
}