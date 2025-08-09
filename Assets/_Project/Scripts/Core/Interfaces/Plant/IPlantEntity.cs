using UniRx;

public interface IPlantEntity
{
    PlantData Data { get; }
    IReadOnlyReactiveProperty<float> GrowthProgress { get; }
    IReadOnlyReactiveProperty<PlantState> State { get; }
    PlantView View { get; }
}