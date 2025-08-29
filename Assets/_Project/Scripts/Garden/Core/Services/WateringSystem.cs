using System;
using System.Collections.Generic;
using UniRx;
using VContainer;

/// <summary>
/// Система полива растений с поддержкой долгого нажатия
/// </summary>
public class WateringSystem : IWateringSystem, IDisposable
{
    private readonly Subject<IPlantEntity> _onPlantWatered = new();
    private readonly Dictionary<IPlantEntity, IDisposable> _activeWaterings = new();
    private readonly CompositeDisposable _disposables = new();
    private readonly GameSettings _gameSettings;

    public IObservable<IPlantEntity> OnPlantWatered => _onPlantWatered;

    [Inject]
    public WateringSystem(GameSettings gameSettings)
    {
        _gameSettings = gameSettings;
    }

    public void Dispose()
    {
        // Остановить все активные поливы
        foreach (var watering in _activeWaterings.Values)
        {
            watering?.Dispose();
        }
        _activeWaterings.Clear();

        _disposables?.Dispose();
        _onPlantWatered?.Dispose();
    }

    public void StartWatering(IPlantEntity plant)
    {
        if (plant == null || !CanWater(plant)) return;

        // Если уже поливаем это растение, ничего не делаем
        if (_activeWaterings.ContainsKey(plant)) return;

        // Показать визуальный эффект начала полива
        plant.View.ShowWateringIcon();

        // Запустить таймер полива
        var wateringTimer = Observable.Timer(TimeSpan.FromSeconds(_gameSettings.WateringDuration))
            .Subscribe(_ =>
            {
                CompleteWatering(plant);
            });

        _activeWaterings[plant] = wateringTimer;
    }

    public void StopWatering(IPlantEntity plant)
    {
        if (plant == null || !_activeWaterings.ContainsKey(plant)) return;

        // Остановить таймер полива
        _activeWaterings[plant]?.Dispose();
        _activeWaterings.Remove(plant);
    }

    public bool CanWater(IPlantEntity plant)
    {
        if (plant == null) return false;

        // Можно поливать растение, если оно ожидает полив (включая сразу после посадки)
        return plant.IsWaitingForWater;
    }

    private void CompleteWatering(IPlantEntity plant)
    {
        if (plant == null) return;

        // Убираем из активных поливов
        if (_activeWaterings.ContainsKey(plant))
        {
            _activeWaterings[plant]?.Dispose();
            _activeWaterings.Remove(plant);
        }

        // Скрываем эффект полива
        plant.View.HideWateringIcon();

        // Поливаем растение
        plant.Water();

        // Показываем эффект успешного полива
        plant.View.PlayWaterSuccessEffect();

        // Уведомляем о завершении полива
        _onPlantWatered.OnNext(plant);
    }
}
