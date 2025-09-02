using UnityEngine;

/// <summary>
/// Менеджер для управления всеми типами механик растений
/// Композитный класс, который координирует выполнение модульных механик
/// </summary>
public class PlantMechanicsManager : IPlantMechanics
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
        _onPlantedMechanics = onPlantedMechanics ?? new OnPlantedMechanics[0];
        _onWateredMechanics = onWateredMechanics ?? new OnWateredMechanics[0];
        _onHarvestedMechanics = onHarvestedMechanics ?? new OnHarvestedMechanics[0];
        _onGrowthStageChangedMechanics = onGrowthStageChangedMechanics ?? new OnGrowthStageChangedMechanics[0];
    }

    public void OnPlanted(IPlantEntity plant, Vector2Int gridPosition)
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

    public void OnWatered(IPlantEntity plant)
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

    public void OnHarvested(IPlantEntity plant, PlantHarvestResult result)
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

    public void OnGrowthStageChanged(IPlantEntity plant, PlantState newState)
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