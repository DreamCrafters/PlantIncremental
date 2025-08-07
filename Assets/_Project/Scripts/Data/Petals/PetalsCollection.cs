using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

[Serializable]
public class PetalsCollection
{
    [SerializeField] private Dictionary<PlantType, int> _petals = new();
    private readonly Subject<PlantType> _onPetalChanged = new();

    public IObservable<PlantType> OnPetalChanged => _onPetalChanged;

    public int GetAmount(PlantType type)
    {
        return _petals.GetValueOrDefault(type, 0);
    }

    public void SetAmount(PlantType type, int amount)
    {
        var previousAmount = GetAmount(type);
        _petals[type] = Mathf.Max(0, amount);
        
        if (previousAmount != _petals[type])
        {
            _onPetalChanged.OnNext(type);
        }
    }

    public void AddAmount(PlantType type, int amount)
    {
        if (amount <= 0) return;
        
        var currentAmount = GetAmount(type);
        SetAmount(type, currentAmount + amount);
    }

    public bool TrySpendAmount(PlantType type, int amount)
    {
        if (amount <= 0) return false;
        
        var currentAmount = GetAmount(type);
        if (currentAmount >= amount)
        {
            SetAmount(type, currentAmount - amount);
            return true;
        }
        return false;
    }

    public bool HasAmount(PlantType type, int amount)
    {
        return GetAmount(type) >= amount;
    }

    public IEnumerable<PetalData> GetAllPetals()
    {
        return _petals.Select(kvp => new PetalData(kvp.Key, kvp.Value));
    }

    public IEnumerable<PetalData> GetNonZeroPetals()
    {
        return _petals.Where(kvp => kvp.Value > 0).Select(kvp => new PetalData(kvp.Key, kvp.Value));
    }

    public void Clear()
    {
        var typesToClear = _petals.Keys.ToList();
        _petals.Clear();
        
        foreach (var type in typesToClear)
        {
            _onPetalChanged.OnNext(type);
        }
    }

    public void ClearType(PlantType type)
    {
        if (_petals.ContainsKey(type))
        {
            _petals.Remove(type);
            _onPetalChanged.OnNext(type);
        }
    }
}
