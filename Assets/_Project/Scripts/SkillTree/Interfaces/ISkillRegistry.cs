/// <summary>
/// Реестр всех навыков
/// </summary>
public interface ISkillRegistry
{
    void RegisterSkillType<T>(SkillType type) where T : ISkill;
    ISkill CreateSkill(SkillData data);
}