using UnityEngine;
using VContainer;

/// <summary>
/// Навык-множитель (монеты, опыт и т.д.)
/// </summary>
public class MultiplierSkill : BaseSkill
{
    [Inject]
    public MultiplierSkill(SkillData data, IEconomyService economy) 
        : base(data, economy)
    {
    }
    
    protected override void ApplyEffect(int level)
    {
        // Эффект применяется через GameParametersService
        // который считывается другими системами
        foreach (var effect in _data.Effects)
        {
            if (effect.Level == level)
            {
                Debug.Log($"Applied multiplier {effect.Value}x to {effect.Parameter}");
            }
        }
    }
    
    public float GetCurrentMultiplier()
    {
        float multiplier = 1f;
        foreach (var effect in _data.Effects)
        {
            if (effect.Level <= _currentLevel.Value && effect.IsMultiplier)
            {
                multiplier *= effect.Value;
            }
        }
        return multiplier;
    }
}