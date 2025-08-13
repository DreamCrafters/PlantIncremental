using UniRx;
using UnityEngine;

/// <summary>
/// Базовая реализация навыка
/// </summary>
public abstract class BaseSkill : ISkill
{
    protected readonly ReactiveProperty<int> _currentLevel = new(0);
    protected readonly SkillData _data;
    protected readonly IEconomyService _economy;
    
    public string Id => _data.Id;
    public string Name => _data.Name;
    public string Description => _data.Description;
    public int MaxLevel => _data.MaxLevel;
    public IReadOnlyReactiveProperty<int> CurrentLevel => _currentLevel;
    
    protected BaseSkill(SkillData data, IEconomyService economy)
    {
        _data = data;
        _economy = economy;
    }
    
    public virtual bool CanUpgrade()
    {
        if (_currentLevel.Value >= MaxLevel) return false;
        
        var cost = GetUpgradeCost(_currentLevel.Value + 1);
        
        // Проверяем монеты
        if (_economy.Coins.Value < cost.Coins) return false;
        
        // Проверяем лепестки если нужны
        if (cost.RequiredPetalType.HasValue && cost.RequiredPetals > 0)
        {
            if (!_economy.HasPetals(cost.RequiredPetalType.Value, cost.RequiredPetals))
                return false;
        }
        
        return true;
    }
    
    public virtual void Upgrade()
    {
        if (!CanUpgrade()) return;
        
        var nextLevel = _currentLevel.Value + 1;
        var cost = GetUpgradeCost(nextLevel);
        
        // Тратим ресурсы
        _economy.TrySpendCoins(cost.Coins);
        if (cost.RequiredPetalType.HasValue && cost.RequiredPetals > 0)
        {
            _economy.TrySpendPetals(cost.RequiredPetalType.Value, cost.RequiredPetals);
        }
        
        // Повышаем уровень
        _currentLevel.Value = nextLevel;
        
        // Применяем эффект
        ApplyEffect(nextLevel);
    }
    
    public SkillUpgradeCost GetUpgradeCost(int toLevel)
    {
        if (_data.LevelCosts == null || _data.LevelCosts.Length == 0)
            return new SkillUpgradeCost(100 * toLevel); // Дефолтная стоимость
        
        // Ищем стоимость для уровня
        foreach (var levelCost in _data.LevelCosts)
        {
            if (levelCost.Level == toLevel)
            {
                return new SkillUpgradeCost(
                    levelCost.CoinCost,
                    levelCost.PetalCost > 0 ? levelCost.RequiredPetalType : null,
                    levelCost.PetalCost
                );
            }
        }
        
        // Если не нашли, используем последнюю с множителем
        var lastCost = _data.LevelCosts[^1];
        var multiplier = toLevel - lastCost.Level + 1;
        return new SkillUpgradeCost(
            lastCost.CoinCost * multiplier,
            lastCost.PetalCost > 0 ? lastCost.RequiredPetalType : null,
            lastCost.PetalCost * multiplier
        );
    }
    
    protected abstract void ApplyEffect(int level);
    
    public void SetLevel(int level)
    {
        _currentLevel.Value = Mathf.Clamp(level, 0, MaxLevel);
        if (level > 0)
        {
            ApplyEffect(level);
        }
    }
}