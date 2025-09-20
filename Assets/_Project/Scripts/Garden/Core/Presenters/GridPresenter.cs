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
    private readonly GridService _gridService;
    private readonly GridView _gridView;
    private readonly WateringManager _wateringManager;
    private readonly EconomyService _economyService;
    private readonly InputService _inputService;
    private readonly GameSettings _settings;

    private readonly CompositeDisposable _disposables = new();
    private readonly Dictionary<Vector2Int, GridCellView> _cellViews = new();

    [Inject]
    public GridPresenter(
        GridService gridService,
        WateringManager wateringManager,
        EconomyService economyService,
        InputService inputService,
        GridView gridView,
        GameSettings settings)
    {
        _gridService = gridService;
        _wateringManager = wateringManager;
        _economyService = economyService;
        _inputService = inputService;
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
        // Отменяем регистрацию всех обработчиков ввода
        foreach (var kvp in _cellViews)
        {
            _inputService.UnregisterCellHandler(kvp.Key);
        }

        _disposables?.Dispose();
    }

    /// <summary>
    /// Инициализирует визуальное представление сетки
    /// </summary>
    private void InitializeGrid()
    {
        var grid = _gridService.Grid.Value;
        if (grid == null) return;

        _gridView.InitializeGrid(_settings.GridSettings.GridSize);

        // Создаем визуальные клетки
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                var position = new Vector2Int(x, y);
                var cell = grid[x, y];
                var cellView = _gridView.CreateCellView(position);

                // Настраиваем длительность долгого нажатия для полива
                if (cellView.TryGetComponent(out LocalInputHandler inputHandler))
                {
                    // Регистрируем обработчик в InputService
                    _inputService.RegisterCellHandler(position, inputHandler);
                }

                _cellViews[position] = cellView;
                UpdateCellVisual(cellView, cell);
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
            .Subscribe(plant => OnWateringCompleted(plant))
            .AddTo(_disposables);

        // Подписка на увядание растений
        _wateringManager.OnPlantWithered
            .Subscribe(plant => OnPlantWithered(plant))
            .AddTo(_disposables);

        // Подписываемся на события ввода через InputService
        SubscribeToAllCellsInput();
    }

    /// <summary>
    /// Подписывается на ввод для всех клеток сетки
    /// </summary>
    private void SubscribeToAllCellsInput()
    {
        // Подписываемся на левую кнопку мыши для всех клеток сетки
        for (int x = 0; x < _settings.GridSettings.GridSize.x; x++)
        {
            for (int y = 0; y < _settings.GridSettings.GridSize.y; y++)
            {
                var position = new Vector2Int(x, y);

                // Правая кнопка мыши - посадка
                _inputService.SubscribeToCellButtonDown(position, PlayerInput.PlantAction, InputTiming.LateTick)
                    .Subscribe(_ => HandleCellRightClick(position))
                    .AddTo(_disposables);

                _inputService.SubscribeToCellButtonComplete(position, PlayerInput.WateringAction, InputTiming.LateTick, _settings.InteractionSettings.WateringDuration)
                    .Subscribe(_ => HandleCellLongPressComplete(position))
                    .AddTo(_disposables);
            }
        }
    }

    /// <summary>
    /// Обрабатывает правый клик по клетке
    /// </summary>
    private void HandleCellRightClick(Vector2Int position)
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
    /// Обрабатывает завершение долгого нажатия на клетке (полив)
    /// </summary>
    private void HandleCellLongPressComplete(Vector2Int position)
    {
        var cell = _gridService.GetCell(position);
        if (cell?.Plant != null && cell.Plant.IsWaitingForWater)
        {
            _wateringManager.WaterPlant(cell.Plant);
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
        _gridService.TryHarvestAt(position);
    }

    /// <summary>
    /// Пытается уничтожить увядшее растение в указанной позиции
    /// </summary>
    private void TryDestroyAt(Vector2Int position)
    {
        _gridService.TryDestroyAt(position);
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
        PlayDestroyEffect(evt.Position, evt.Plant);

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
    /// Обработка события увядания растения
    /// </summary>
    private void OnPlantWithered(PlantEntity plant)
    {
        // Можем добавить визуальные эффекты или уведомления
    }

    /// <summary>
    /// Обработка завершения полива (мгновенный полив)
    /// </summary>
    private void OnWateringCompleted(PlantEntity plant)
    {
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
    private void PlayDestroyEffect(Vector2Int position, PlantEntity plant)
    {
        if (_cellViews.TryGetValue(position, out var cellView))
        {
            cellView.PlayDestroyEffect();

            if (plant?.View != null)
            {
                Debug.Log("Playing destroy animation on plant view");
                plant.View.PlayDestroyAnimation();
            }
        }
    }
}