using VContainer;

/// <summary>
/// Навык разблокировки функции
/// </summary>
public class UnlockSkill : BaseSkill
{
    private readonly IUnlockService _unlockService;
    
    [Inject]
    public UnlockSkill(SkillData data, IEconomyService economy, IUnlockService unlockService) 
        : base(data, economy)
    {
        _unlockService = unlockService;
    }
    
    protected override void ApplyEffect(int level)
    {
        // Разблокируем функцию
        if (level > 0)
        {
            _unlockService.Unlock(_data.Id);
        }
    }
}