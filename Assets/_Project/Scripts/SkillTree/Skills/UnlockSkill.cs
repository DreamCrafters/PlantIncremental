using UnityEngine;

public class UnlockSkill : Skill
{
    [Header("Unlock Skill")]
    [SerializeField] private Type _type;

    protected override void UpgradeHandle()
    {
    }

    public enum Type
    {
        None,
    }
}
