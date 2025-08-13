using UniRx;

/// <summary>
/// Базовый интерфейс навыка
/// </summary>
public interface ISkill
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    int MaxLevel { get; }
    IReadOnlyReactiveProperty<int> CurrentLevel { get; }
    
    bool CanUpgrade();
    void Upgrade();
    SkillUpgradeCost GetUpgradeCost(int toLevel);
}