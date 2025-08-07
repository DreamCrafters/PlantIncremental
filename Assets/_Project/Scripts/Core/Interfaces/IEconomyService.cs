using System;
using UniRx;

public interface IEconomyService
{
    IReadOnlyReactiveProperty<int> Coins { get; }
    PetalsCollection PetalsCollection { get; }
    IObservable<PlantType> OnPetalChanged { get; }
    
    void AddCoins(int amount);
    bool TrySpendCoins(int amount);
    
    void AddPetals(PlantType type, int amount);
    bool TrySpendPetals(PlantType type, int amount);
    bool HasPetals(PlantType type, int amount);
    int GetPetalsAmount(PlantType type);
    
    PetalsCollectionSaveData GetPetalsSaveData();
    void LoadPetalsData(PetalsCollectionSaveData saveData);
}