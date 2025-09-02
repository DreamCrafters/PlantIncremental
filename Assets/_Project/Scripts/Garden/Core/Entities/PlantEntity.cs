using System;
using UniRx;
using UnityEngine;

/// <summary>
/// Основная сущность растения, представляющая модель данных с реактивными свойствами
/// </summary>
public class PlantEntity : IPlantEntity
{
    private readonly ReactiveProperty<float> _growthProgress = new(0f);
    private readonly ReactiveProperty<PlantState> _state = new(PlantState.New);
    private readonly ReactiveProperty<bool> _isWaitingForWater = new(true);
    private readonly CompositeDisposable _disposables = new();

    private readonly PlantView _view;
    private readonly IPlantMechanics _mechanics;
    private Vector2Int _gridPosition;

    public PlantData Data { get; }
    public IReadOnlyReactiveProperty<float> GrowthProgress => _growthProgress;
    public IReadOnlyReactiveProperty<PlantState> State => _state;
    public PlantView View => _view;
    public bool IsWaitingForWater => _isWaitingForWater.Value;
    public bool IsHarvestable => _state.Value == PlantState.FullyGrown;
    public bool IsWithered => _state.Value == PlantState.Withered;
    public Vector2 Position => _view.transform.position;
    public Vector2Int GridPosition => _gridPosition;

    public PlantEntity(PlantData data, PlantView view, IPlantMechanics mechanics)
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _mechanics = mechanics ?? throw new ArgumentNullException(nameof(mechanics));

        SubscribeToStateChanges();
        UpdateVisual();
    }

    public void Dispose()
    {
        _disposables?.Dispose();
        if (_view != null)
        {
            UnityEngine.Object.Destroy(_view.gameObject);
        }
    }

    public void UpdateGrowthProgress(float progress)
    {
        if (progress < 0f || progress > 1f)
        {
            Debug.LogWarning($"Invalid growth progress: {progress}. Must be between 0 and 1");
            progress = Mathf.Clamp01(progress);
        }
        
        _growthProgress.Value = progress;
    }

    public void UpdateState(PlantState newState)
    {
        if (_state.Value != newState)
        {
            _state.Value = newState;
            
            try
            {
                _mechanics.OnGrowthStageChanged(this, newState);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error executing growth stage changed mechanics: {ex.Message}");
            }
        }
    }

    public void SetWaitingForWater(bool waiting)
    {
        if (_isWaitingForWater.Value != waiting)
        {
            _isWaitingForWater.Value = waiting;
            
            if (waiting)
            {
                _view.ShowWateringIcon();
            }
            else
            {
                _view.HideWateringIcon();
            }
        }
    }

    public void SetGridPosition(Vector2Int gridPosition)
    {
        _gridPosition = gridPosition;
        
        try
        {
            _mechanics.OnPlanted(this, gridPosition);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error executing planted mechanics: {ex.Message}");
        }
    }

    public PlantHarvestResult Harvest()
    {
        if (!IsHarvestable)
        {
            Debug.LogWarning($"Cannot harvest plant in state {_state.Value}");
            return new PlantHarvestResult { Success = false };
        }

        var result = new PlantHarvestResult
        {
            Success = true,
            Coins = CalculateCoinsReward(),
            Petals = CalculatePetalsReward(),
        };

        try
        {
            // Вызываем механики сбора урожая
            _mechanics.OnHarvested(this, result);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error executing harvested mechanics: {ex.Message}");
        }

        // Анимация сбора
        _view.PlayHarvestAnimation();

        return result;
    }

    public void Water()
    {
        _isWaitingForWater.Value = false;
    }

    public void ActivatePassiveAbility()
    {
        if (_state.Value != PlantState.FullyGrown)
        {
            return;
        }

        try
        {
            _view.ShowPassiveEffect();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error activating passive ability: {ex.Message}");
        }
    }

    public void TriggerWaterMechanics()
    {
        try
        {
            _mechanics.OnWatered(this);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error executing watered mechanics: {ex.Message}");
        }
    }

    private void SubscribeToStateChanges()
    {
        _state.Subscribe(state =>
        {
            UpdateVisual();
            
            // Обновляем визуальные индикаторы в зависимости от состояния
            switch (state)
            {
                case PlantState.FullyGrown:
                    OnFullyGrown();
                    break;

                case PlantState.Withered:
                    OnWithered();
                    break;
            }
        }).AddTo(_disposables);

        _isWaitingForWater.Subscribe(isWaiting =>
        {
            if (isWaiting)
            {
                _view.ShowWateringIcon();
            }
            else
            {
                _view.HideWateringIcon();
            }
        }).AddTo(_disposables);
        
        // Подписываемся на изменения прогресса роста для обновления визуала
        _growthProgress.Subscribe(_ => UpdateVisual()).AddTo(_disposables);
    }

    private void UpdateVisual()
    {
        if (_view == null || Data.GrowthStages == null) return;

        try
        {
            var stageIndex = (int)_state.Value;
            if (stageIndex >= 0 && stageIndex < Data.GrowthStages.Length)
            {
                _view.UpdateSprite(Data.GrowthStages[stageIndex].Sprite);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error updating plant visual: {ex.Message}");
        }
    }

    private void OnFullyGrown()
    {
        ActivatePassiveAbility();
    }

    private void OnWithered()
    {
        try
        {
            _view.PlayWitherEffect();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error playing wither effect: {ex.Message}");
        }
    }

    private int CalculateCoinsReward()
    {
        return Data.SellPrice;
    }

    private PetalData CalculatePetalsReward()
    {
        return new PetalData(Data.Type, 1);
    }
}