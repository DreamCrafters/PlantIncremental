using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PetalsCollectionSaveData
{
    [SerializeField] private List<PetalSaveData> _petals = new();

    public List<PetalSaveData> Petals => _petals;

    public static PetalsCollectionSaveData FromCollection(PetalsCollection collection)
    {
        var saveData = new PetalsCollectionSaveData();
        
        foreach (var petal in collection.GetNonZeroPetals())
        {
            saveData._petals.Add(new PetalSaveData
            {
                type = petal.Type,
                amount = petal.Amount
            });
        }
        
        return saveData;
    }

    public void ApplyToCollection(PetalsCollection collection)
    {
        collection.Clear();
        
        foreach (var petalData in _petals)
        {
            collection.SetAmount(petalData.type, petalData.amount);
        }
    }
}