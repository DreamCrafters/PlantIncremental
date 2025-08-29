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
    public float InteractionCooldown => _settings.InteractionCooldown;
    public float LastInteractionTime => _lastInteractionTime;

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
        if (IsValidPosition(position) == false) return null;
        return _grid.Value[position.x, position.y];
    }

    public bool TryPlantAt(Vector2Int position)
    {
        if (IsAbleToInteract() == false) return false;

        var cell = GetCell(position);

        if (cell == null || !cell.IsEmpty) return false;

        var plantData = GetRandomPlantData();

        if (plantData == null || _settings.ViewPrefab == null) return false;

        var plant = _plantFactory.CreatePlant(plantData);

        if (cell.TryPlant(plant))
        {
            _grid.SetValueAndForceNotify(_grid.Value);
            _lastInteractionTime = Time.time;
            return true;
        }

        return false;
    }

    public bool TryHarvestAt(Vector2Int position)
    {
        if (IsAbleToInteract() == false) return false;

        var cell = GetCell(position);

        if (cell == null || cell.IsEmpty) return false;

        var plant = cell.Plant;

        if (CanHarvest(plant) == false) return false;

        var harvestedPlant = cell.Harvest();

        if (harvestedPlant != null)
        {
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

        return false;
    }

    public bool TryDestroyAt(Vector2Int position)
    {
        if (IsAbleToInteract() == false) return false;

        var cell = GetCell(position);

        if (cell == null || cell.IsEmpty) return false;

        var plant = cell.Plant;

        if (CanDestroy(plant) == false) return false;

        var destroyedPlant = cell.Harvest(); // Используем Harvest() для удаления из ячейки

        if (destroyedPlant != null)
        {
            _onPlantDestroyed.OnNext(new PlantDestroyedEvent
            {
                Plant = destroyedPlant,
                Position = position
            });

            _grid.SetValueAndForceNotify(_grid.Value);
            _lastInteractionTime = Time.time;
            return true;
        }

        return false;
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
        if (random < 0.6f) return SoilType.Fertile;
        if (random < 0.9f) return SoilType.Rocky;
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
        return rarityChances[rarityChances.Length - 1].Rarity;
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