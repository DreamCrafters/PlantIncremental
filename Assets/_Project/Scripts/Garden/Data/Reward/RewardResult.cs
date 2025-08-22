/// <summary>
/// Результат получения награды
/// </summary>
public struct RewardResult
{
    public int Coins { get; set; }
    public PetalReward Petals { get; set; }

    public static RewardResult Empty => new RewardResult
    {
        Coins = 0,
        Petals = new PetalReward { Type = PlantType.Basic, Amount = 0 }
    };
}