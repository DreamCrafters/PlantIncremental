using System;

[Serializable]
public class SaveData
{
    private EconomySaveData _economyData;

    public void SetEconomyData(EconomySaveData economyData)
    {
        _economyData = economyData;
    }

    public EconomySaveData GetEconomyData()
    {
        if (_economyData != null)
        {
            return _economyData;
        }

        PetalsCollection petalsCollection = new();
        petalsCollection.SetAmount(PlantType.Basic, 0);

        return EconomySaveData.Create(0, petalsCollection);
    }
}