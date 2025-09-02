using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Компонент для обработки долгих нажатий мышкой/касанием
/// </summary>
public class LongPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ILongPressHandler
{
    private readonly Subject<Unit> _onLongPressStart = new();
    private readonly Subject<Unit> _onLongPressEnd = new();
    private readonly Subject<Unit> _onLongPressComplete = new();
    
    private IDisposable _longPressTimer;
    private bool _isPressed;
    private float _longPressDuration = 1f;
    
    public IObservable<Unit> OnLongPressStart => _onLongPressStart;
    public IObservable<Unit> OnLongPressEnd => _onLongPressEnd;
    public IObservable<Unit> OnLongPressComplete => _onLongPressComplete;
    
    public float LongPressDuration
    {
        get => _longPressDuration;
        set => _longPressDuration = Mathf.Max(0.1f, value);
    }

    private void OnDestroy()
    {
        _longPressTimer?.Dispose();
        _onLongPressStart?.Dispose();
        _onLongPressEnd?.Dispose();
        _onLongPressComplete?.Dispose();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isPressed || eventData.button != PointerEventData.InputButton.Left) return;
        
        _isPressed = true;
        _onLongPressStart.OnNext(Unit.Default);
        
        // Запускаем таймер долгого нажатия
        _longPressTimer?.Dispose();
        _longPressTimer = Observable.Timer(TimeSpan.FromSeconds(_longPressDuration))
            .Subscribe(_ =>
            {
                if (_isPressed)
                {
                    _onLongPressComplete.OnNext(Unit.Default);
                }
            });
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isPressed || eventData.button != PointerEventData.InputButton.Left) return;
        
        _isPressed = false;
        _onLongPressEnd.OnNext(Unit.Default);
        
        // Останавливаем таймер
        _longPressTimer?.Dispose();
        _longPressTimer = null;
    }
    
    /// <summary>
    /// Программно отменяет долгое нажатие
    /// </summary>
    public void CancelLongPress()
    {
        if (_isPressed)
        {
            _isPressed = false;
            _onLongPressEnd.OnNext(Unit.Default);
            _longPressTimer?.Dispose();
            _longPressTimer = null;
        }
    }
    
    /// <summary>
    /// Перезапускает таймер долгого нажатия если нажатие активно
    /// </summary>
    public void RestartLongPress()
    {
        if (!_isPressed) return;
        
        // Перезапускаем таймер без изменения состояния
        _longPressTimer?.Dispose();
        _longPressTimer = Observable.Timer(TimeSpan.FromSeconds(_longPressDuration))
            .Subscribe(_ =>
            {
                if (_isPressed)
                {
                    _onLongPressComplete.OnNext(Unit.Default);
                }
            });
    }
    
    /// <summary>
    /// Программно начинает долгое нажатие
    /// </summary>
    public void StartLongPress()
    {
        if (_isPressed) return;
        
        _isPressed = true;
        _onLongPressStart.OnNext(Unit.Default);
        
        // Запускаем таймер долгого нажатия
        _longPressTimer?.Dispose();
        _longPressTimer = Observable.Timer(TimeSpan.FromSeconds(_longPressDuration))
            .Subscribe(_ =>
            {
                if (_isPressed)
                {
                    _onLongPressComplete.OnNext(Unit.Default);
                }
            });
    }
    
    /// <summary>
    /// Получить текущее состояние нажатия
    /// </summary>
    public bool IsPressed => _isPressed;
    
    /// <summary>
    /// Завершает текущее долгое нажатие и сбрасывает состояние для возможности начать новое
    /// </summary>
    public void CompleteAndReset()
    {
        if (_isPressed)
        {
            _isPressed = false;
            _longPressTimer?.Dispose();
            _longPressTimer = null;
        }
    }
}
