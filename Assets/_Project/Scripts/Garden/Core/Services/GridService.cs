using System;
using UniRx;
using UnityEngine;
using VContainer;

public class GridService : IGridService, IDisposable
{
    private readonly ReactiveProperty<GridCell[,]> _grid;
    private readonly Subject<PlantHarvestedEvent> _onPlantHarvested = new();
    private readonly Subject<PlantDestroyedEvent> _onPlantDestroyed = new();
    private readonly CompositeDisposable _disposables = new();

    private readonly GameSettings _settings;
    private readonly IPlantFactory _plantFactory;
    private readonly IRewardService _rewardService;

    private float _lastInteractionTime;

    public IReadOnlyReactiveProperty<GridCell[,]> Grid => _grid;
    public IObservable<PlantHarvestedEvent> OnPlantHarvested => _onPlantHarvested;
    public IObservable<PlantDestroyedEvent> OnPlantDestroyed => _onPlantDestroyed;

    [Inject]
    public GridService(GameSettings settings, IPlantFactory plantFactory, IRewardService rewardService)
    {
        _settings = settings;
        _plantFactory = plantFactory;
        _rewardService = rewardService;
        _grid = new ReactiveProperty<GridCell[,]>(InitializeGrid());
    }

    public void Dispose()
    {
        _disposables?.Dispose();
        _onPlantHarvested?.Dispose();
        _onPlantDestroyed?.Dispose();
    }

    public GridCell GetCell(Vector2Int position)
    {
        if (!IsValidPosition(position))
        {
            Debug.LogWarning($"Invalid grid position requested: {position}. Grid size: {_settings.GridSize}");
            return null;
        }

        try
        {
            return _grid.Value[position.x, position.y];
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to get cell at position {position}: {ex.Message}");
            return null;
        }
    }

    public bool TryPlantAt(Vector2Int position)
    {
        if (!IsAbleToInteract())
        {
            Debug.LogWarning("Grid interaction is on cooldown");
            return false;
        }

        var cell = GetCell(position);
        if (cell == null)
        {
            return false; // Error already logged in GetCell
        }

        if (!cell.IsEmpty)
        {
            Debug.LogWarning($"Cannot plant at {position}: cell is not empty");
            return false;
        }

        var plantData = GetRandomPlantData();
        if (plantData == null)
        {
            Debug.LogError("No plant data available for planting");
            return false;
        }

        if (_settings.ViewPrefab == null)
        {
            Debug.LogError("ViewPrefab is not configured in GameSettings");
            return false;
        }

        try
        {
            var plant = _plantFactory.CreatePlant(plantData);
            if (plant == null)
            {
                Debug.LogError($"PlantFactory failed to create plant for {plantData.name}");
                return false;
            }

            if (cell.TryPlant(plant))
            {
                _grid.SetValueAndForceNotify(_grid.Value);
                _lastInteractionTime = Time.time;
                return true;
            }
            else
            {
                Debug.LogWarning($"Failed to plant {plantData.name} at {position}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception while planting at {position}: {ex.Message}");
            return false;
        }
    }

    public bool TryHarvestAt(Vector2Int position)
    {
        if (!IsAbleToInteract())
        {
            Debug.LogWarning("Grid interaction is on cooldown");
            return false;
        }

        var cell = GetCell(position);
        if (cell == null)
        {
            return false; // Error already logged in GetCell
        }

        if (cell.IsEmpty)
        {
            Debug.LogWarning($"Cannot harvest at {position}: cell is empty");
            return false;
        }

        var plant = cell.Plant;
        if (plant == null)
        {
            Debug.LogError($"Cell at {position} reports not empty but plant is null");
            return false;
        }

        if (!CanHarvest(plant))
        {
            Debug.LogWarning($"Cannot harvest plant at {position}: plant state is {plant.State.Value}");
            return false;
        }

        try
        {
            var harvestedPlant = cell.Harvest();
            if (harvestedPlant == null)
            {
                Debug.LogError($"Harvest failed at {position}: returned null plant");
                return false;
            }

            // Обрабатываем награды через RewardService
            var reward = _rewardService.ProcessHarvest(harvestedPlant);

            _onPlantHarvested.OnNext(new PlantHarvestedEvent
            {
                Plant = harvestedPlant,
                Position = position,
                Reward = reward
            });

            _grid.SetValueAndForceNotify(_grid.Value);
            _lastInteractionTime = Time.time;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception while harvesting at {position}: {ex.Message}");
            return false;
        }
    }

    public bool TryDestroyAt(Vector2Int position)
    {
        if (!IsAbleToInteract())
        {
            Debug.LogWarning("Grid interaction is on cooldown");
            return false;
        }

        var cell = GetCell(position);
        if (cell == null)
        {
            return false; // Error already logged in GetCell
        }

        if (cell.IsEmpty)
        {
            Debug.LogWarning($"Cannot destroy at {position}: cell is empty");
            return false;
        }

        var plant = cell.Plant;
        if (plant == null)
        {
            Debug.LogError($"Cell at {position} reports not empty but plant is null");
            return false;
        }

        if (!CanDestroy(plant))
        {
            Debug.LogWarning($"Cannot destroy plant at {position}: plant state is {plant.State.Value} (only withered plants can be destroyed)");
            return false;
        }

        try
        {
            var destroyedPlant = cell.Harvest(); // Используем Harvest() для удаления из ячейки
            if (destroyedPlant == null)
            {
                Debug.LogError($"Destroy failed at {position}: returned null plant");
                return false;
            }

            _onPlantDestroyed.OnNext(new PlantDestroyedEvent
            {
                Plant = destroyedPlant,
                Position = position
            });

            _grid.SetValueAndForceNotify(_grid.Value);
            _lastInteractionTime = Time.time;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception while destroying plant at {position}: {ex.Message}");
            return false;
        }
    }

    public GridCell[] GetNeighbors(Vector2Int position, int radius = 1)
    {
        var neighbors = new System.Collections.Generic.List<GridCell>();

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x == 0 && y == 0) continue;

                var neighborPos = new Vector2Int(position.x + x, position.y + y);
                var neighbor = GetCell(neighborPos);

                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors.ToArray();
    }

    /// <summary>
    /// Улучшает тип почвы в указанной позиции
    /// </summary>
    public bool TryImproveSoil(Vector2Int position)
    {
        var cell = GetCell(position);

        if (cell == null || !cell.IsEmpty) return false;

        SoilType newSoilType = (SoilType)Mathf.Max((int)SoilType.Fertile, (int)cell.SoilType - 1);

        if (newSoilType != cell.SoilType)
        {
            cell.SoilType = newSoilType;
            _grid.SetValueAndForceNotify(_grid.Value);
            return true;
        }

        return false;
    }

    private bool IsAbleToInteract()
    {
        return Time.time - _lastInteractionTime > _settings.InteractionCooldown;
    }

    private GridCell[,] InitializeGrid()
    {
        var size = _settings.GridSize;
        var grid = new GridCell[size.x, size.y];

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                grid[x, y] = new GridCell(new Vector2Int(x, y), GenerateSoilType());
            }
        }

        return grid;
    }

    private SoilType GenerateSoilType()
    {
        var random = UnityEngine.Random.Range(0f, 1f);
        var soilChances = _settings.GetNormalizedSoilTypeChances();
        
        foreach (var soilChance in soilChances)
        {
            if (random < soilChance.Chance)
            {
                return soilChance.Type;
            }

            random -= soilChance.Chance;
        }

        return SoilType.Unsuitable;
    }

    private PlantData GetRandomPlantData()
    {
        var plants = _settings.AvailablePlants;
        if (plants == null || plants.Length == 0) return null;

        // Получаем случайную редкость с учётом шансов
        var selectedRarity = GetRandomRarity();
        
        // Фильтруем растения по выбранной редкости
        var plantsOfRarity = Array.FindAll(plants, p => p != null && p.Rarity == selectedRarity);
        
        // Если растений выбранной редкости нет, возвращаем случайное растение
        if (plantsOfRarity.Length == 0)
        {
            int fallbackIndex = UnityEngine.Random.Range(0, plants.Length);
            return plants[fallbackIndex];
        }
        
        // Возвращаем случайное растение выбранной редкости
        int index = UnityEngine.Random.Range(0, plantsOfRarity.Length);
        return plantsOfRarity[index];
    }

    private PlantRarity GetRandomRarity()
    {
        var rarityChances = _settings.GetNormalizedRarityChances();
        if (rarityChances == null || rarityChances.Length == 0)
        {
            // Если настройки редкости не заданы, возвращаем Common
            return PlantRarity.Common;
        }

        float randomValue = UnityEngine.Random.Range(0f, 1f);
        float cumulativeChance = 0f;

        foreach (var rarityChance in rarityChances)
        {
            cumulativeChance += rarityChance.Chance;
            if (randomValue <= cumulativeChance)
            {
                return rarityChance.Rarity;
            }
        }

        // Возвращаем последнюю редкость если ничего не выбралось (защита от ошибок конфигурации)
        return rarityChances[^1].Rarity;
    }

    private bool CanHarvest(IPlantEntity plant)
    {
        return plant.State.Value == PlantState.FullyGrown;
    }

    private bool CanDestroy(IPlantEntity plant)
    {
        return plant.State.Value == PlantState.Withered;
    }

    private bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < _settings.GridSize.x &&
               position.y >= 0 && position.y < _settings.GridSize.y;
    }
}