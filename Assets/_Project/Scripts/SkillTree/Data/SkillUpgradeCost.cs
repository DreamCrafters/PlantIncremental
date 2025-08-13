using System;

/// <summary>
/// Стоимость улучшения навыка
/// </summary>
[Serializable]
public struct SkillUpgradeCost
{
    public int Coins;
    public PlantType? RequiredPetalType;
    public int RequiredPetals;
    
    public SkillUpgradeCost(int coins, PlantType? petalType = null, int petals = 0)
    {
        Coins = coins;
        RequiredPetalType = petalType;
        RequiredPetals = petals;
    }
}