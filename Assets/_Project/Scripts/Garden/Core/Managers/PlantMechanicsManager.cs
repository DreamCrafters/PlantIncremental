using UnityEngine;

/// <summary>
/// Менеджер для управления всеми типами механик растений
/// Композитный класс, который координирует выполнение модульных механик
/// </summary>
public class PlantMechanicsManager
{
    private readonly OnPlantedMechanics[] _onPlantedMechanics;
    private readonly OnWateredMechanics[] _onWateredMechanics;
    private readonly OnHarvestedMechanics[] _onHarvestedMechanics;
    private readonly OnGrowthStageChangedMechanics[] _onGrowthStageChangedMechanics;

    public PlantMechanicsManager(
        OnPlantedMechanics[] onPlantedMechanics,
        OnWateredMechanics[] onWateredMechanics,
        OnHarvestedMechanics[] onHarvestedMechanics,
        OnGrowthStageChangedMechanics[] onGrowthStageChangedMechanics)
    {
        _onPlantedMechanics = onPlantedMechanics;
        _onWateredMechanics = onWateredMechanics;
        _onHarvestedMechanics = onHarvestedMechanics;
        _onGrowthStageChangedMechanics = onGrowthStageChangedMechanics;
    }

    public void OnPlanted(PlantEntity plant, Vector2Int gridPosition)
    {
        foreach (var mechanic in _onPlantedMechanics)
        {
            if (mechanic != null)
            {
                try
                {
                    mechanic.Execute(plant, gridPosition);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Ошибка при выполнении механики посадки {mechanic.name}: {ex.Message}");
                }
            }
        }
    }

    public void OnWatered(PlantEntity plant)
    {
        foreach (var mechanic in _onWateredMechanics)
        {
            if (mechanic != null)
            {
                try
                {
                    mechanic.Execute(plant);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Ошибка при выполнении механики полива {mechanic.name}: {ex.Message}");
                }
            }
        }
    }

    public void OnHarvested(PlantEntity plant, PlantHarvestResult result)
    {
        foreach (var mechanic in _onHarvestedMechanics)
        {
            if (mechanic != null)
            {
                try
                {
                    mechanic.Execute(plant, result);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Ошибка при выполнении механики сбора {mechanic.name}: {ex.Message}");
                }
            }
        }
    }

    public void OnGrowthStageChanged(PlantEntity plant, PlantState newState)
    {
        foreach (var mechanic in _onGrowthStageChangedMechanics)
        {
            if (mechanic != null)
            {
                try
                {
                    mechanic.Execute(plant, newState);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Ошибка при выполнении механики роста {mechanic.name}: {ex.Message}");
                }
            }
        }
    }
}