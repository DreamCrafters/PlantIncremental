using VContainer;

/// <summary>
/// Навык улучшения параметра (скорость роста, доход и т.д.)
/// </summary>
public class ParameterSkill : BaseSkill
{
    private readonly IGameParametersService _parameters;
    
    [Inject]
    public ParameterSkill(SkillData data, IEconomyService economy, IGameParametersService parameters) 
        : base(data, economy)
    {
        _parameters = parameters;
    }
    
    protected override void ApplyEffect(int level)
    {
        // Находим эффект для уровня
        foreach (var effect in _data.Effects)
        {
            if (effect.Level <= level)
            {
                _parameters.ApplyModifier(
                    effect.Parameter,
                    effect.Value,
                    effect.IsMultiplier
                );
            }
        }
    }
}