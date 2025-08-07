using System;
using UniRx;

public class EconomyService : IEconomyService
{
    private readonly ReactiveProperty<int> _coins = new();
    private readonly PetalsCollection _petalsCollection = new();

    public IReadOnlyReactiveProperty<int> Coins => _coins;
    public PetalsCollection PetalsCollection => _petalsCollection;
    public IObservable<PlantType> OnPetalChanged => _petalsCollection.OnPetalChanged;

    public void AddCoins(int amount)
    {
        _coins.Value += amount;
    }

    public bool TrySpendCoins(int amount)
    {
        if (_coins.Value >= amount)
        {
            _coins.Value -= amount;
            return true;
        }
        return false;
    }

    public void AddPetals(PlantType type, int amount)
    {
        _petalsCollection.AddAmount(type, amount);
    }

    public bool TrySpendPetals(PlantType type, int amount)
    {
        return _petalsCollection.TrySpendAmount(type, amount);
    }

    public bool HasPetals(PlantType type, int amount)
    {
        return _petalsCollection.HasAmount(type, amount);
    }

    public int GetPetalsAmount(PlantType type)
    {
        return _petalsCollection.GetAmount(type);
    }

    public PetalsCollectionSaveData GetPetalsSaveData()
    {
        return PetalsCollectionSaveData.FromCollection(_petalsCollection);
    }

    public void LoadPetalsData(PetalsCollectionSaveData saveData)
    {
        saveData.ApplyToCollection(_petalsCollection);
    }
}
