using UnityEngine;

public class ExtraCoinsSkill : Skill
{
    [SerializeField] private Type _type;

    protected override void UpgradeHandle()
    {
    }

    public enum Type
    {
        PerHarvest,
        PerSecondByGrowedPlants
    }
}
