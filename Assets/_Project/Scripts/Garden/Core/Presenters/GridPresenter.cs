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
    private readonly GridView _gridView;
    private readonly IWateringManager _wateringManager;
    private readonly IEconomyService _economyService;
    private readonly GameSettings _settings;

    private readonly CompositeDisposable _disposables = new();
    private readonly Dictionary<Vector2Int, GridCellView> _cellViews = new();

    [Inject]
    public GridPresenter(
        IGridService gridService,
        IWateringManager wateringManager,
        IEconomyService economyService,
        GridView gridView,
        GameSettings settings)
    {
        _gridService = gridService;
        _wateringManager = wateringManager;
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

        // Подписка на уничтожение растений
        _gridService.OnPlantDestroyed
            .Subscribe(evt => OnPlantDestroyed(evt))
            .AddTo(_disposables);
            
        // Подписка на завершение полива из WateringManager
        _wateringManager.OnPlantWatered
            .Subscribe(plant => OnPlantWatered(plant))
            .AddTo(_disposables);
            
        // Подписка на увядание растений
        _wateringManager.OnPlantWithered
            .Subscribe(plant => OnPlantWithered(plant))
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
            
        // Подписываемся на события полива
        cellView.OnWateringStart
            .Subscribe(plant => OnWateringStarted(plant))
            .AddTo(_disposables);
            
        cellView.OnWateringEnd
            .Subscribe(plant => OnWateringEnded(plant))
            .AddTo(_disposables);
            
        // Подписываемся на завершение долгого нажатия (мгновенный полив)
        cellView.OnWateringComplete
            .Subscribe(plant => OnWateringCompleted(plant))
            .AddTo(_disposables);
    }

    /// <summary>
    /// Обрабатывает клик по клетке
    /// </summary>
    private void HandleCellClick(Vector2Int position)
    {
        var cell = _gridService.GetCell(position);
        if (cell == null) return;

        if (cell.IsEmpty && cell.SoilType != SoilType.Unsuitable)
        {
            // Пытаемся посадить растение
            TryPlantAt(position);
        }
        else if (cell.Plant != null)
        {
            switch (cell.Plant.State.Value)
            {
                case PlantState.FullyGrown:
                    // Пытаемся собрать урожай
                    TryHarvestAt(position);
                    break;
                case PlantState.Withered:
                    // Пытаемся уничтожить увядшее растение
                    TryDestroyAt(position);
                    break;
            }
        }
    }

    /// <summary>
    /// Пытается посадить растение в указанную позицию
    /// </summary>
    private void TryPlantAt(Vector2Int position)
    {
        if (_gridService.TryPlantAt(position))
        {
            PlayPlantEffect(position);
        }
    }

    /// <summary>
    /// Пытается собрать урожай в указанной позиции
    /// </summary>
    private void TryHarvestAt(Vector2Int position)
    {
        if (_gridService.TryHarvestAt(position))
        {
            PlayHarvestEffect(position);
        }
    }

    /// <summary>
    /// Пытается уничтожить увядшее растение в указанной позиции
    /// </summary>
    private void TryDestroyAt(Vector2Int position)
    {
        if (_gridService.TryDestroyAt(position))
        {
            PlayDestroyEffect(position);
        }
    }

    /// <summary>
    /// Обработка изменения сетки
    /// </summary>
    private void OnGridChanged(GridCell[,] grid)
    {
        if (grid == null) return;

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
        
        // Если в клетке есть растение, привязываем его entity к клетке
        if (cell.IsEmpty == false && cell.Plant != null)
        {
            cellView.SetPlantEntity(cell.Plant);
        }
        else
        {
            cellView.SetPlantEntity(null);
        }
    }

    /// <summary>
    /// Обработка события сбора урожая
    /// </summary>
    private void OnPlantHarvested(PlantHarvestedEvent evt)
    {
        // Только визуальные эффекты и UI логика
        var reward = evt.Reward;
        
        // Показываем всплывающее сообщение о награде
        ShowRewardPopup(evt.Position, reward);
        
        // Визуальный эффект сбора урожая
        PlayHarvestEffect(evt.Position);
    }

    /// <summary>
    /// Обработка события уничтожения увядшего растения
    /// </summary>
    private void OnPlantDestroyed(PlantDestroyedEvent evt)
    {
        // Визуальные эффекты уничтожения
        PlayDestroyEffect(evt.Position);
        
        // Дополнительно можно показать уведомление игроку
        // например "Увядшее растение удалено"
    }

    /// <summary>
    /// Показывает всплывающее окно с наградой
    /// </summary>
    private void ShowRewardPopup(Vector2Int position, RewardResult result)
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
    /// Обработка события завершения полива растения
    /// </summary>
    private void OnPlantWatered(IPlantEntity plant)
    {
    }
    
    /// <summary>
    /// Обработка события увядания растения
    /// </summary>
    private void OnPlantWithered(IPlantEntity plant)
    {
        // Можем добавить визуальные эффекты или уведомления
        Debug.Log($"Plant withered at position {plant.Position}");
    }
    
    /// <summary>
    /// Обработка начала полива
    /// </summary>
    private void OnWateringStarted(IPlantEntity plant)
    {
        // Здесь можно добавить визуальные эффекты начала полива
    }
    
    /// <summary>
    /// Обработка окончания полива
    /// </summary>
    private void OnWateringEnded(IPlantEntity plant)
    {
        // Здесь можно добавить визуальные эффекты окончания полива
    }
    
    /// <summary>
    /// Обработка завершения полива (мгновенный полив)
    /// </summary>
    private void OnWateringCompleted(IPlantEntity plant)
    {
        // Поливаем растение через менеджер
        _wateringManager.WaterPlant(plant);
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

    /// <summary>
    /// Воспроизводит эффект уничтожения увядшего растения
    /// </summary>
    private void PlayDestroyEffect(Vector2Int position)
    {
        if (_cellViews.TryGetValue(position, out var cellView))
        {
            cellView.PlayDestroyEffect();
            var cell = _gridService.GetCell(position);

            if (cell?.Plant?.View != null)
            {
                cell.Plant.View.PlayDestroyAnimation();
            }
        }
    }
}