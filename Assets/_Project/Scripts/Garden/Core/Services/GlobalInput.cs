using System;
using UniRx;
using UnityEngine;
using VContainer.Unity;

/// <summary>
/// Реализация сервиса для обработки глобального ввода
/// </summary>
public class GlobalInput : ITickable, ILateTickable
{
    private readonly Camera _camera;
    private readonly CompositeDisposable _disposables = new();
    
    // Реактивные свойства
    private readonly ReactiveProperty<bool> _isMainButtonPressed = new(false);
    private readonly ReactiveProperty<Vector2> _screenPosition = new(Vector2.zero);
    private readonly ReactiveProperty<Vector3> _worldPosition = new(Vector3.zero);
    
    // События
    private readonly Subject<Vector2> _onMainButtonDown = new();
    private readonly Subject<Vector2> _onMainButtonUp = new();
    private readonly Subject<Vector2> _onPointerMove = new();
    
    // Состояние
    private bool _wasPressed;
    
    public IReadOnlyReactiveProperty<bool> IsMainButtonPressed => _isMainButtonPressed;
    public IReadOnlyReactiveProperty<Vector2> ScreenPosition => _screenPosition;
    public IReadOnlyReactiveProperty<Vector3> WorldPosition => _worldPosition;
    public IObservable<Vector2> OnMainButtonDown => _onMainButtonDown;
    public IObservable<Vector2> OnMainButtonUp => _onMainButtonUp;
    public IObservable<Vector2> OnPointerMove => _onPointerMove;
    
    public GlobalInput()
    {
        _camera = Camera.main;
        
        // Инициализируем начальные значения
        UpdateInputState();
        
        // Настраиваем очистку ресурсов
        _isMainButtonPressed.AddTo(_disposables);
        _screenPosition.AddTo(_disposables);
        _worldPosition.AddTo(_disposables);
        _onMainButtonDown.AddTo(_disposables);
        _onMainButtonUp.AddTo(_disposables);
        _onPointerMove.AddTo(_disposables);
    }
    
    public void Tick()
    {
    }

    public void LateTick()
    {
        UpdateInputState();
    }
    
    private void UpdateInputState()
    {
        // Получаем текущую позицию мыши
        Vector2 currentScreenPos = Input.mousePosition;
        Vector3 currentWorldPos = _camera.ScreenToWorldPoint(new Vector3(currentScreenPos.x, currentScreenPos.y, _camera.nearClipPlane));
        currentWorldPos.z = 0; // Фиксируем Z для 2D игры

        // Проверяем нажатие основной кнопки мыши
        bool isPressed = Input.GetMouseButton(0);

        // Обновляем позиции если они изменились
        if (Vector2.Distance(_screenPosition.Value, currentScreenPos) > 0.1f)
        {
            _screenPosition.Value = currentScreenPos;
            _worldPosition.Value = currentWorldPos;
            _onPointerMove.OnNext(currentScreenPos);
        }

        // Обрабатываем нажатие кнопки
        if (isPressed != _wasPressed)
        {
            _isMainButtonPressed.Value = isPressed;

            if (isPressed)
            {
                _onMainButtonDown.OnNext(currentScreenPos);
            }
            else
            {
                _onMainButtonUp.OnNext(currentScreenPos);
            }

            _wasPressed = isPressed;
        }
    }
    
    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
