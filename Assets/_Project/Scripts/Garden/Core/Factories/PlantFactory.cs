using System;
using UnityEngine;
using VContainer;

public class PlantFactory : IPlantFactory
{
    private readonly IObjectResolver _resolver;
    private readonly GameSettings _settings;
    private readonly IPlantMechanicsFactory _mechanicsFactory;

    [Inject]
    public PlantFactory(IObjectResolver resolver, GameSettings settings, IPlantMechanicsFactory mechanicsFactory)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _mechanicsFactory = mechanicsFactory ?? throw new ArgumentNullException(nameof(mechanicsFactory));
    }

    public IPlantEntity CreatePlant(PlantData data)
    {
        if (data == null)
        {
            Debug.LogError("Cannot create plant with null PlantData");
            return null;
        }

        if (_settings.ViewPrefab == null)
        {
            Debug.LogError("ViewPrefab is not configured in GameSettings");
            return null;
        }

        try
        {
            // Создаем view без установки позиции - позицией будет управлять клетка
            var view = UnityEngine.Object.Instantiate(_settings.ViewPrefab);
            if (view == null)
            {
                Debug.LogError("Failed to instantiate plant view prefab");
                return null;
            }

            // Создаем механики через фабрику
            var mechanics = _mechanicsFactory.CreateMechanics(data);
            if (mechanics == null)
            {
                Debug.LogError("Failed to create plant mechanics");
                UnityEngine.Object.Destroy(view.gameObject);
                return null;
            }

            // Создаем сущность растения
            var entity = new PlantEntity(data, view, mechanics);
            
            // Внедряем зависимости через VContainer (если потребуется в будущем)
            _resolver.Inject(entity);

            return entity;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception while creating plant: {ex.Message}");
            return null;
        }
    }
}