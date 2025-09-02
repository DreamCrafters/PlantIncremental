using System;
using UniRx;
using UnityEngine;

public class EconomyService : IEconomyService
{
    private readonly ReactiveProperty<int> _coins = new();
    private readonly PetalsCollection _petalsCollection = new();

    public IReadOnlyReactiveProperty<int> Coins => _coins;
    public PetalsCollection PetalsCollection => _petalsCollection;
    public IObservable<PlantType> OnPetalChanged => _petalsCollection.OnPetalChanged;

    public void AddCoins(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"Attempting to add negative coins: {amount}");
            return;
        }

        if (amount == 0)
        {
            return; // Nothing to add
        }

        var newValue = _coins.Value + amount;
        if (newValue < _coins.Value) // Overflow check
        {
            Debug.LogError($"Coin overflow detected! Current: {_coins.Value}, Adding: {amount}");
            _coins.Value = int.MaxValue;
        }
        else
        {
            _coins.Value = newValue;
        }
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"Attempting to spend negative coins: {amount}");
            return false;
        }

        if (amount == 0)
        {
            return true; // Nothing to spend
        }

        if (_coins.Value >= amount)
        {
            _coins.Value -= amount;
            return true;
        }
        
        Debug.LogWarning($"Insufficient coins. Required: {amount}, Available: {_coins.Value}");
        return false;
    }

    public void AddPetals(PlantType type, int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"Attempting to add negative petals: {amount} for type {type}");
            return;
        }

        if (amount == 0)
        {
            return; // Nothing to add
        }

        try
        {
            _petalsCollection.AddAmount(type, amount);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to add petals ({type}, {amount}): {ex.Message}");
        }
    }

    public bool TrySpendPetals(PlantType type, int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"Attempting to spend negative petals: {amount} for type {type}");
            return false;
        }

        if (amount == 0)
        {
            return true; // Nothing to spend
        }

        try
        {
            return _petalsCollection.TrySpendAmount(type, amount);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to spend petals ({type}, {amount}): {ex.Message}");
            return false;
        }
    }

    public bool HasPetals(PlantType type, int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"Checking for negative petals amount: {amount} for type {type}");
            return false;
        }

        try
        {
            return _petalsCollection.HasAmount(type, amount);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to check petals ({type}, {amount}): {ex.Message}");
            return false;
        }
    }

    public int GetPetalsAmount(PlantType type)
    {
        try
        {
            return _petalsCollection.GetAmount(type);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to get petals amount for type {type}: {ex.Message}");
            return 0;
        }
    }

    public EconomySaveData GetSaveData()
    {
        try
        {
            return EconomySaveData.Create(_coins.Value, _petalsCollection);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create economy save data: {ex.Message}");
            return new EconomySaveData(); // Return empty save data as fallback
        }
    }

    public void LoadData(EconomySaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogWarning("Attempting to load null save data, skipping");
            return;
        }

        try
        {
            saveData.Apply(_petalsCollection, out int coins);
            
            if (coins < 0)
            {
                Debug.LogWarning($"Loaded negative coins value: {coins}, setting to 0");
                coins = 0;
            }
            
            _coins.Value = coins;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load economy data: {ex.Message}");
            // Keep current values instead of corrupting the state
        }
    }
}
