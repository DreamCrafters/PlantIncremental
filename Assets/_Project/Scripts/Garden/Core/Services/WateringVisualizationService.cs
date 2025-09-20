using System;
using UniRx;
using UnityEngine;
using VContainer;

/// <summary>
/// Реализация сервиса для управления визуальными эффектами полива
/// </summary>
public class WateringVisualizationService
{
    private readonly InputService _inputService;
    private readonly CompositeDisposable _disposables = new();
    
    // Реактивные свойства
    private readonly ReactiveProperty<bool> _isWateringVisualizationActive = new(false);
    private readonly ReactiveProperty<Vector3> _wateringCursorWorldPosition = new(Vector3.zero);
    
    // События
    private readonly Subject<Unit> _onWateringVisualizationStarted = new();
    private readonly Subject<Unit> _onWateringVisualizationStopped = new();
    
    [Inject]
    public WateringVisualizationService(InputService inputService)
    {
        _inputService = inputService;
        Initialize();
    }
    
    public IReadOnlyReactiveProperty<bool> IsWateringVisualizationActive => _isWateringVisualizationActive;
    public IReadOnlyReactiveProperty<Vector3> WateringCursorWorldPosition => _wateringCursorWorldPosition;
    public IObservable<Unit> OnWateringVisualizationStarted => _onWateringVisualizationStarted;
    public IObservable<Unit> OnWateringVisualizationStopped => _onWateringVisualizationStopped;
    
    private void Initialize()
    {
        // Настраиваем автоматическое управление визуализацией по нажатию мыши
        _inputService.SubscribeToButtonDown(PlayerInput.WateringAction, InputTiming.Tick)
            .Subscribe(_ => StartWateringVisualization())
            .AddTo(_disposables);

        _inputService.SubscribeToButtonUp(PlayerInput.WateringAction, InputTiming.Tick)
            .Subscribe(_ => StopWateringVisualization())
            .AddTo(_disposables);
        
        // Обновляем позицию курсора полива при движении мыши, но только когда визуализация активна
        _inputService.WorldPositionLate
            .Where(_ => _isWateringVisualizationActive.Value)
            .Subscribe(worldPos => UpdateWateringCursorPosition(worldPos))
            .AddTo(_disposables);
        
        // Настраиваем очистку ресурсов
        _isWateringVisualizationActive.AddTo(_disposables);
        _wateringCursorWorldPosition.AddTo(_disposables);
        _onWateringVisualizationStarted.AddTo(_disposables);
        _onWateringVisualizationStopped.AddTo(_disposables);
    }
    
    public void StartWateringVisualization()
    {
        if (!_isWateringVisualizationActive.Value)
        {
            _isWateringVisualizationActive.Value = true;
            _wateringCursorWorldPosition.Value = _inputService.WorldPositionLate.Value;
            _onWateringVisualizationStarted.OnNext(Unit.Default);
        }
    }
    
    public void StopWateringVisualization()
    {
        if (_isWateringVisualizationActive.Value)
        {
            _isWateringVisualizationActive.Value = false;
            _onWateringVisualizationStopped.OnNext(Unit.Default);
        }
    }
    
    public void UpdateWateringCursorPosition(Vector3 worldPosition)
    {
        _wateringCursorWorldPosition.Value = worldPosition;
    }
    
    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
