using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class PlantEntity : IPlantEntity
{
    public PlantData Data => throw new System.NotImplementedException();
    public IReadOnlyReactiveProperty<float> GrowthProgress => throw new System.NotImplementedException();
    public IReadOnlyReactiveProperty<PlantState> State => throw new System.NotImplementedException();

    public PlantEntity(PlantData data, PlantView view)
    {
        throw new System.NotImplementedException();
    }
}
