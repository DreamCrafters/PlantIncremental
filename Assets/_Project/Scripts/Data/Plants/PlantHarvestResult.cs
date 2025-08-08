/// <summary>
/// Результат сбора растения
/// </summary>
public struct PlantHarvestResult
{
    public bool Success;
    public int Coins;
    public PetalData Petals;
    public BonusItem[] BonusItems;

    public struct BonusItem
    {
        public Type BonusItemType;
        public int Amount;

        public enum Type
        {
            Seed,
            Fertilizer,
            WateringCanUpgrade,
            RareSeed
        }
    }
}