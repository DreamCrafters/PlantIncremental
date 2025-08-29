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
    private readonly IPlantGrowthService _growthService;
    private readonly IEconomyService _economyService;
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

        // Подписка на уничтожение растений
        _gridService.OnPlantDestroyed
            .Subscribe(evt => OnPlantDestroyed(evt))
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
    /// Пытается уничтожить увядшее растение в указанной позиции
    /// </summary>
    private void TryDestroyAt(Vector2Int position)
    {
        if (_gridService.TryDestroyAt(position))
        {
            // Растение успешно уничтожено - GridService уже отправил событие
            PlayDestroyEffect(position);
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
        // Только визуальные эффекты и UI логика
        var reward = evt.Reward;
        
        // Показываем всплывающее сообщение о награде
        ShowRewardPopup(evt.Position, reward);
        
        // Визуальный эффект сбора урожая
        PlayHarvestEffect(evt.Position);

        // Останавливаем рост (растение уже собрано)
        _growthService.StopGrowing(evt.Plant);
    }

    /// <summary>
    /// Обработка события уничтожения увядшего растения
    /// </summary>
    private void OnPlantDestroyed(PlantDestroyedEvent evt)
    {
        // Визуальные эффекты уничтожения
        PlayDestroyEffect(evt.Position);

        // Останавливаем рост (растение уже уничтожено)
        _growthService.StopGrowing(evt.Plant);
        
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
            
            // Также проигрываем анимацию уничтожения на самом растении
            var cell = _gridService.GetCell(position);
            if (cell?.Plant?.View != null)
            {
                cell.Plant.View.PlayDestroyAnimation();
            }
        }
    }
}