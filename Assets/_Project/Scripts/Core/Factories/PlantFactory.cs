using UnityEngine;
using VContainer;

public interface IPlantFactory
{
    // IPlantEntity CreatePlant(PlantData data, Vector2 position);
}

public class PlantFactory : IPlantFactory
{
    // private readonly IInstantiator _instantiator;
    // private readonly PlantView.Pool _plantPool;
    
    // [Inject]
    // public PlantFactory(IInstantiator instantiator, PlantView.Pool plantPool)
    // {
    //     _instantiator = instantiator;
    //     _plantPool = plantPool;
    // }
    
    // public IPlantEntity CreatePlant(PlantData data, Vector2 position)
    // {
    //     var view = _plantPool.Rent();
    //     view.transform.position = position;
        
    //     var entity = new PlantEntity(data, view);
    //     _instantiator.Inject(entity);
        
    //     return entity;
    // }
}