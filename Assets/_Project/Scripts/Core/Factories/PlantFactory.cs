using UnityEngine;
using VContainer;

public class PlantFactory : IPlantFactory
{
    private readonly IObjectResolver _resolver;
    private readonly GameSettings _settings;
    
    [Inject]
    public PlantFactory(IObjectResolver resolver, GameSettings settings)
    {
        _resolver = resolver;
        _settings = settings;
    }
    
    public IPlantEntity CreatePlant(PlantData data, Vector2 position)
    {
        // Создаем view без установки позиции - позицией будет управлять клетка
        var view = Object.Instantiate(_settings.ViewPrefab);
        
        var entity = new PlantEntity(data, view);
        _resolver.Inject(entity);
        
        return entity;
    }
}