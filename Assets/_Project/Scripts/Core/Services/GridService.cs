using System;
using UniRx;
using UnityEngine;
using VContainer;

public class GridService : IGridService, IDisposable
{
    private readonly ReactiveProperty<GridCell[,]> _grid;
    private readonly Subject<GridCell> _onCellClicked = new();
    private readonly Subject<PlantHarvestedEvent> _onPlantHarvested = new();
    private readonly CompositeDisposable _disposables = new();

    private readonly GameSettings _settings;
    private readonly IPlantFactory _plantFactory;

    public IReadOnlyReactiveProperty<GridCell[,]> Grid => _grid;
    public IObservable<GridCell> OnCellClicked => _onCellClicked;
    public IObservable<PlantHarvestedEvent> OnPlantHarvested => _onPlantHarvested;

    [Inject]
    public GridService(GameSettings settings, IPlantFactory plantFactory)
    {
        _settings = settings;
        _plantFactory = plantFactory;
        _grid = new ReactiveProperty<GridCell[,]>(InitializeGrid());
    }

    private GridCell[,] InitializeGrid()
    {
        var size = _settings.GridSize;
        var grid = new GridCell[size.x, size.y];

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                grid[x, y] = new GridCell(new Vector2Int(x, y), GenerateSoilType(x, y));
            }
        }

        return grid;
    }

    private SoilType GenerateSoilType(int x, int y)
    {
        // Улучшенная генерация с паттернами
        var random = UnityEngine.Random.Range(0f, 1f);
        
        // Центр сетки более плодородный
        var center = new Vector2(_settings.GridSize.x / 2f, _settings.GridSize.y / 2f);
        var distance = Vector2.Distance(new Vector2(x, y), center);
        var maxDistance = Vector2.Distance(Vector2.zero, center);
        var centerBonus = 1f - (distance / maxDistance) * 0.3f;
        
        random *= centerBonus;

        if (random < 0.6f) return SoilType.Fertile;
        if (random < 0.9f) return SoilType.Rocky;
        return SoilType.Unsuitable;
    }

    public GridCell GetCell(Vector2Int position)
    {
        if (IsValidPosition(position) == false) return null;
        return _grid.Value[position.x, position.y];
    }

    public bool TryPlantAt(Vector2Int position, PlantData data)
    {
        var cell = GetCell(position);
        if (cell == null || !cell.IsEmpty) return false;

        if (CanPlantOnSoil(data, cell.SoilType) == false) return false;

        var worldPosition = GridToWorldPosition(position);
        var plant = _plantFactory.CreatePlant(data, worldPosition);

        if (cell.TryPlant(plant))
        {
            _grid.SetValueAndForceNotify(_grid.Value);
            return true;
        }

        return false;
    }

    public bool TryHarvestAt(Vector2Int position)
    {
        var cell = GetCell(position);
        if (cell == null || cell.IsEmpty) return false;

        var plant = cell.Plant;

        if (CanHarvest(plant) == false) return false;

        var harvestedPlant = cell.Harvest();

        if (harvestedPlant != null)
        {
            _onPlantHarvested.OnNext(new PlantHarvestedEvent
            {
                Plant = harvestedPlant,
                Position = position,
                Reward = harvestedPlant.Data.SellPrice
            });

            _grid.SetValueAndForceNotify(_grid.Value);
            return true;
        }

        return false;
    }

    private bool CanPlantOnSoil(PlantData plant, SoilType soil)
    {
        if (soil == SoilType.Unsuitable) return false;

        // Некоторые растения могут расти только на определенной почве
        // TODO: Добавить в PlantData предпочтительные типы почвы
        return true;
    }

    private bool CanHarvest(IPlantEntity plant)
    {
        return plant.State.Value == PlantState.FullyGrown;
    }

    private Vector2 GridToWorldPosition(Vector2Int gridPos)
    {
        // Преобразование координат сетки в мировые координаты
        const float cellSize = 1f;
        const float offsetX = -2.5f; // Центрирование сетки 6x6
        const float offsetY = -2.5f;

        return new Vector2(
            gridPos.x * cellSize + offsetX,
            gridPos.y * cellSize + offsetY
        );
    }

    private bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < _settings.GridSize.x &&
               position.y >= 0 && position.y < _settings.GridSize.y;
    }

    public void HandleCellClick(Vector2Int position)
    {
        var cell = GetCell(position);
        if (cell != null)
        {
            _onCellClicked.OnNext(cell);
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

        var newSoilType = cell.SoilType switch
        {
            SoilType.Unsuitable => SoilType.Rocky,
            SoilType.Rocky => SoilType.Fertile,
            SoilType.Fertile => SoilType.Fertile, // Уже максимальный уровень
            _ => cell.SoilType
        };

        if (newSoilType != cell.SoilType)
        {
            cell.SoilType = newSoilType;
            _grid.SetValueAndForceNotify(_grid.Value);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Получает статистику сетки
    /// </summary>
    public GridStatistics GetStatistics()
    {
        var stats = new GridStatistics();
        var grid = _grid.Value;

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                var cell = grid[x, y];
                
                stats.TotalCells++;
                
                if (cell.IsEmpty)
                {
                    stats.EmptyCells++;
                }
                else
                {
                    stats.OccupiedCells++;
                    
                    if (cell.Plant.State.Value == PlantState.FullyGrown)
                    {
                        stats.ReadyToHarvest++;
                    }
                    else if (cell.Plant.State.Value == PlantState.Withered)
                    {
                        stats.WitheredPlants++;
                    }
                }

                switch (cell.SoilType)
                {
                    case SoilType.Fertile:
                        stats.FertileCells++;
                        break;
                    case SoilType.Rocky:
                        stats.RockyCells++;
                        break;
                    case SoilType.Unsuitable:
                        stats.UnsuitableCells++;
                        break;
                }
            }
        }

        return stats;
    }

    public void Dispose()
    {
        _disposables?.Dispose();
        _onCellClicked?.Dispose();
        _onPlantHarvested?.Dispose();
    }
}