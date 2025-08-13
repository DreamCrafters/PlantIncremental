/// <summary>
/// Фабрика для создания навыков
/// </summary>
public interface ISkillFactory
{
    ISkill CreateSkill(SkillData data);
}