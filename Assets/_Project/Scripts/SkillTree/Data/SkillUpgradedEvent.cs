/// <summary>
/// Событие улучшения навыка
/// </summary>
public struct SkillUpgradedEvent
{
    public string TreeId;
    public string NodeId;
    public string SkillId;
    public int NewLevel;
}