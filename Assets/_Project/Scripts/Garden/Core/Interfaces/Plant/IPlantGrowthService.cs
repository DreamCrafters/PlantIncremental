using System;

public interface IPlantGrowthService
{
    void StartGrowing(IPlantEntity plant);
    void StopGrowing(IPlantEntity plant);
    IObservable<IPlantEntity> OnPlantGrown { get; }
}