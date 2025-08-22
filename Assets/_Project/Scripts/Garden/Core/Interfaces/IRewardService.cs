/// <summary>
/// Сервис для расчета и выдачи наград за сбор урожая
/// </summary>
public interface IRewardService
{
    /// <summary>
    /// Обрабатывает сбор урожая и выдает награды
    /// </summary>
    RewardResult ProcessHarvest(IPlantEntity plant);
}