using VContainer;

/// <summary>
/// Реализация сервиса наград
/// </summary>
public class RewardService
{
    private readonly EconomyService _economyService;

    [Inject]
    public RewardService(EconomyService economyService)
    {
        _economyService = economyService;
    }

    public RewardResult ProcessHarvest(PlantEntity plant)
    {
        if (plant is not PlantEntity entity)
        {
            return RewardResult.Empty;
        }

        var harvestResult = entity.Harvest();
        
        // Добавляем монеты в экономику
        if (harvestResult.Coins > 0)
        {
            _economyService.AddCoins(harvestResult.Coins);
        }
        
        // Добавляем лепестки в экономику
        if (harvestResult.Petals.Amount > 0)
        {
            _economyService.AddPetals(harvestResult.Petals.Type, harvestResult.Petals.Amount);
        }

        // Возвращаем результат для UI
        return new RewardResult
        {
            Coins = harvestResult.Coins,
            Petals = new PetalReward
            {
                Type = harvestResult.Petals.Type,
                Amount = harvestResult.Petals.Amount
            }
        };
    }
}
