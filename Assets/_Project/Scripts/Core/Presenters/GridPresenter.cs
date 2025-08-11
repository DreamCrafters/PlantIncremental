using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Презентер для управления визуализацией игровой сетки
/// </summary>
public class GridPresenter : IInitializable, IDisposable
{
    private readonly IGridService _gridService;
    private readonly IPlantGrowthService _growthService;
    private readonly IEconomyService _economyService;
    private readonly GridView _gridView;
    private readonly GameSettings _settings;

    private readonly CompositeDisposable _disposables = new();
    private readonly Dictionary<Vector2Int, GridCellView> _cellViews = new();

    [Inject]
    public GridPresenter(
        IGridService gridService,
        IPlantGrowthService growthService,
        IEconomyService economyService,
        GridView gridView,
        GameSettings settings)
    {
        _gridService = gridService;
        _growthService = growthService;
        _economyService = economyService;
        _gridView = gridView;
        _settings = settings;
    }

    public void Initialize()
    {
        InitializeGrid();
        SubscribeToEvents();
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }

    /// <summary>
    /// Инициализирует визуальное представление сетки
    /// </summary>
    private void InitializeGrid()
    {
        var grid = _gridService.Grid.Value;
        if (grid == null) return;

        _gridView.InitializeGrid(_settings.GridSize);

        // Создаем визуальные клетки
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                var position = new Vector2Int(x, y);
                var cell = grid[x, y];
                var cellView = _gridView.CreateCellView(position);

                _cellViews[position] = cellView;
                UpdateCellVisual(cellView, cell);
                SubscribeToCellClick(cellView, position);
            }
        }

        _gridView.AnimateGridAppearance();
    }

    /// <summary>
    /// Подписывается на события системы
    /// </summary>
    private void SubscribeToEvents()
    {
        // Подписка на изменения сетки
        _gridService.Grid
            .Subscribe(grid => OnGridChanged(grid))
            .AddTo(_disposables);

        // Подписка на сбор урожая
        _gridService.OnPlantHarvested
            .Subscribe(evt => OnPlantHarvested(evt))
            .AddTo(_disposables);
    }

    /// <summary>
    /// Подписывается на клики по конкретной клетке
    /// </summary>
    private void SubscribeToCellClick(GridCellView cellView, Vector2Int position)
    {
        cellView.OnClick
            .Subscribe(_ => HandleCellClick(position))
            .AddTo(_disposables);
    }

    /// <summary>
    /// Обрабатывает клик по клетке
    /// </summary>
    private void HandleCellClick(Vector2Int position)
    {
        var cell = _gridService.GetCell(position);
        if (cell == null) return;

        if (cell.IsEmpty)
        {
            // Пытаемся посадить растение
            TryPlantAt(position);
        }
        else if (cell.Plant.State.Value == PlantState.FullyGrown)
        {
            // Пытаемся собрать урожай
            TryHarvestAt(position);
        }
    }

    /// <summary>
    /// Пытается посадить растение в указанную позицию
    /// </summary>
    private void TryPlantAt(Vector2Int position)
    {
        // Пытаемся посадить
        if (_gridService.TryPlantAt(position))
        {
            // Запускаем рост
            var cell = _gridService.GetCell(position);
            if (cell?.Plant != null)
            {
                _growthService.StartGrowing(cell.Plant);
            }

            // Визуальный эффект посадки
            PlayPlantEffect(position);
        }
        else
        {
            ShowCannotPlantMessage(position);
        }
    }

    /// <summary>
    /// Пытается собрать урожай в указанной позиции
    /// </summary>
    private void TryHarvestAt(Vector2Int position)
    {
        if (_gridService.TryHarvestAt(position))
        {
            // Урожай собран успешно - GridService уже отправил событие
            PlayHarvestEffect(position);
        }
    }

    /// <summary>
    /// Обработка изменения сетки
    /// </summary>
    private void OnGridChanged(GridCell[,] grid)
    {
        if (grid == null) return;

        // Обновляем визуальное представление всех клеток
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                var position = new Vector2Int(x, y);
                if (_cellViews.TryGetValue(position, out var cellView))
                {
                    UpdateCellVisual(cellView, grid[x, y]);
                }
            }
        }
    }

    /// <summary>
    /// Обновляет визуал конкретной клетки
    /// </summary>
    private void UpdateCellVisual(GridCellView cellView, GridCell cell)
    {
        cellView.UpdateVisual(cell);
        cellView.ShowPlant(cell.IsEmpty == false);
        
        // Если в клетке есть растение, привязываем его view к клетке
        if (cell.IsEmpty == false && cell.Plant != null)
        {
            cellView.SetPlantView(cell.Plant.View);
        }
        else
        {
            // Если клетка пустая, очищаем растение
            cellView.SetPlantView(null);
        }
    }

    /// <summary>
    /// Обработка события сбора урожая
    /// </summary>
    private void OnPlantHarvested(PlantHarvestedEvent evt)
    {
        int coinsToAdd = 0;

        if (evt.Plant is PlantEntity entity)
        {
            PlantHarvestResult harvestResult = entity.Harvest();
            coinsToAdd = harvestResult.Coins;

            // Добавляем лепестки
            if (harvestResult.Petals.Amount > 0)
            {
                _economyService.AddPetals(harvestResult.Petals.Type, harvestResult.Petals.Amount);
            }

            // Показываем всплывающее сообщение о награде
            ShowRewardPopup(evt.Position, harvestResult);
        }

        // Добавляем монеты один раз после получения результата
        if (coinsToAdd > 0)
        {
            _economyService.AddCoins(coinsToAdd);
        }

        // Останавливаем рост (уже собрано)
        _growthService.StopGrowing(evt.Plant);
    }

    /// <summary>
    /// Показывает сообщение о невозможности посадки
    /// </summary>
    private void ShowCannotPlantMessage(Vector2Int position)
    {
        var cell = _gridService.GetCell(position);
        var message = cell?.SoilType == SoilType.Unsuitable
            ? "Непригодная почва!"
            : "Нельзя посадить здесь!";

        _gridView.ShowMessage(message, MessageType.Warning);
    }

    /// <summary>
    /// Показывает всплывающее окно с наградой
    /// </summary>
    private void ShowRewardPopup(Vector2Int position, PlantHarvestResult result)
    {
        if (_cellViews.TryGetValue(position, out var cellView))
        {
            _gridView.ShowRewardPopup(
                cellView.transform.position,
                result.Coins,
                result.Petals.Amount
            );
        }
    }

    /// <summary>
    /// Воспроизводит эффект посадки
    /// </summary>
    private void PlayPlantEffect(Vector2Int position)
    {
        if (_cellViews.TryGetValue(position, out var cellView))
        {
            cellView.PlayPlantEffect();
        }
    }

    /// <summary>
    /// Воспроизводит эффект сбора урожая
    /// </summary>
    private void PlayHarvestEffect(Vector2Int position)
    {
        if (_cellViews.TryGetValue(position, out var cellView))
        {
            cellView.PlayHarvestEffect();
        }
    }
}