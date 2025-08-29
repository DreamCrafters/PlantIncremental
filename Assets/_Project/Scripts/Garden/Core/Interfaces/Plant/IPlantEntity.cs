using UniRx;

public interface IPlantEntity
{
    PlantData Data { get; }
    IReadOnlyReactiveProperty<float> GrowthProgress { get; }
    IReadOnlyReactiveProperty<PlantState> State { get; }
    PlantView View { get; }
    bool IsWaitingForWater { get; }

    /// <summary>
    /// Поливает растение, позволяя ему продолжить рост
    /// </summary>
    void Water();
    
    /// <summary>
    /// Время, прошедшее с последнего полива (в секундах)
    /// </summary>
    float TimeSinceLastWatering { get; }
}