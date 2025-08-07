using UnityEngine;
using VContainer;

public class PlantFactory : IPlantFactory
{
    private readonly IObjectResolver _resolver;
    
    [Inject]
    public PlantFactory(IObjectResolver resolver)
    {
        _resolver = resolver;
    }
    
    public IPlantEntity CreatePlant(PlantData data, Vector2 position)
    {
        var view = Object.Instantiate(data.ViewPrefab);
        view.transform.position = position;
        
        var entity = new PlantEntity(data, view);
        _resolver.Inject(entity);
        
        return entity;
    }
}