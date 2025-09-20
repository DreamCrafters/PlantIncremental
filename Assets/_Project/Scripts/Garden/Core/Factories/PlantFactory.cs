using System;
using UnityEngine;
using VContainer;

public class PlantFactory
{
    private readonly IObjectResolver _resolver;
    private readonly GameSettings _settings;
    private readonly PlantMechanicsFactory _mechanicsFactory;

    [Inject]
    public PlantFactory(IObjectResolver resolver, GameSettings settings, PlantMechanicsFactory mechanicsFactory)
    {
        _resolver = resolver;
        _settings = settings;
        _mechanicsFactory = mechanicsFactory;
    }

    public PlantEntity CreatePlant(PlantData data)
    {
        if (data == null)
        {
            Debug.LogError("Cannot create plant with null PlantData");
            return null;
        }

        if (_settings.PlantSettings?.ViewPrefab == null)
        {
            Debug.LogError("ViewPrefab is not configured in PlantSettings");
            return null;
        }

        try
        {
            // Создаем view без установки позиции - позицией будет управлять клетка
            var view = UnityEngine.Object.Instantiate(_settings.PlantSettings.ViewPrefab);
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