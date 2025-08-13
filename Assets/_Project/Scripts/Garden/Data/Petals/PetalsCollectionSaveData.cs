using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EconomySaveData
{
    [SerializeField] private int _coins;
    [SerializeField] private List<PetalSaveData> _petals = new();

    public static EconomySaveData Create(int coins, PetalsCollection collection)
    {
        var saveData = new EconomySaveData
        {
            _coins = coins
        };

        foreach (var petal in collection.GetAllPetals())
        {
            saveData._petals.Add(new PetalSaveData
            {
                type = petal.Type,
                amount = petal.Amount
            });
        }

        return saveData;
    }

    public void Apply(PetalsCollection collection, out int coins)
    {
        coins = _coins;
        collection.Clear();
        
        foreach (var petalData in _petals)
        {
            collection.SetAmount(petalData.type, petalData.amount);
        }
    }
}