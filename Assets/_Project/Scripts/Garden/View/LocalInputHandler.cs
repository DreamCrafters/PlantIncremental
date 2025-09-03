using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Универсальный компонент для обработки локального ввода - кликов, наведения курсора, долгих нажатий и перетаскивания
/// </summary>
public class LocalInputHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    // События кликов и наведения
    private readonly Subject<PointerEventData> _onClick = new();
    private readonly Subject<PointerEventData> _onPointerEnter = new();
    private readonly Subject<PointerEventData> _onPointerExit = new();
    
    // События долгого нажатия
    private readonly Subject<Unit> _onLongPressStart = new();
    private readonly Subject<Unit> _onLongPressEnd = new();
    private readonly Subject<Unit> _onLongPressComplete = new();
    
    // Состояние взаимодействия
    private IDisposable _longPressTimer;
    private bool _isPressed;
    private bool _isMouseOver;
    private bool _isDragging;
    private float _longPressDuration = 1f;
    
    // Настройки
    [Header("Input Settings")]
    [SerializeField] private bool _enableLongPress = true;
    [SerializeField] private bool _enableDragToStart = true;
    [SerializeField] private float _longPressDurationOverride = -1f; // -1 означает использовать значение по умолчанию

    // Публичные события
    public IObservable<PointerEventData> OnClick => _onClick;
    public IObservable<PointerEventData> OnPointerEntered => _onPointerEnter;
    public IObservable<PointerEventData> OnPointerExited => _onPointerExit;
    public IObservable<Unit> OnLongPressStart => _onLongPressStart;
    public IObservable<Unit> OnLongPressEnd => _onLongPressEnd;
    public IObservable<Unit> OnLongPressComplete => _onLongPressComplete;
    
    // Публичные свойства
    public float LongPressDuration
    {
        get => _longPressDurationOverride > 0 ? _longPressDurationOverride : _longPressDuration;
        set => _longPressDuration = Mathf.Max(0.1f, value);
    }
    
    public bool IsPressed => _isPressed;
    public bool IsMouseOver => _isMouseOver;
    public bool IsDragging => _isDragging;
    public bool EnableLongPress
    {
        get => _enableLongPress;
        set => _enableLongPress = value;
    }
    public bool EnableDragToStart
    {
        get => _enableDragToStart;
        set => _enableDragToStart = value;
    }

    private void Awake()
    {
        // Если указана переопределяющая длительность в инспекторе, используем её
        if (_longPressDurationOverride > 0)
        {
            _longPressDuration = _longPressDurationOverride;
        }
    }

    private void Update()
    {
        // Обновляем состояние drag
        if (Input.GetMouseButton(0))
        {
            _isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isDragging = false;
            
            // Завершаем долгое нажатие если оно было активно
            if (_isPressed)
            {
                var fakeEventData = new PointerEventData(EventSystem.current)
                {
                    button = PointerEventData.InputButton.Left,
                    position = Input.mousePosition
                };
                OnPointerUp(fakeEventData);
            }
        }
        
        // Автоматический запуск долгого нажатия при drag, если это разрешено
        if (_enableDragToStart && _isDragging && _isMouseOver && !_isPressed && _enableLongPress)
        {
            StartLongPress();
        }
    }

    private void OnDestroy()
    {
        _longPressTimer?.Dispose();
        _onClick?.Dispose();
        _onPointerEnter?.Dispose();
        _onPointerExit?.Dispose();
        _onLongPressStart?.Dispose();
        _onLongPressEnd?.Dispose();
        _onLongPressComplete?.Dispose();
    }

    #region IPointer Interface Implementation
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        
        // Отправляем событие клика
        _onClick.OnNext(eventData);
        
        // Запускаем долгое нажатие если оно включено
        if (_enableLongPress && !_isPressed)
        {
            _isPressed = true;
            _onLongPressStart.OnNext(Unit.Default);
            
            StartLongPressTimer();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isPressed || eventData.button != PointerEventData.InputButton.Left) return;
        
        _isPressed = false;
        _onLongPressEnd.OnNext(Unit.Default);
        
        // Останавливаем таймер
        StopLongPressTimer();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isMouseOver = true;
        _onPointerEnter.OnNext(eventData);
        
        // Если долгое нажатие активно - перезапускаем таймер
        if (_isPressed && _enableLongPress)
        {
            RestartLongPress();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isMouseOver = false;
        _onPointerExit.OnNext(eventData);
        
        // При уходе курсора отменяем долгое нажатие в любом случае
        if (_isPressed)
        {
            CancelLongPress();
        }
    }
    
    #endregion

    #region Long Press Control Methods
    
    /// <summary>
    /// Программно начинает долгое нажатие
    /// </summary>
    public void StartLongPress()
    {
        if (_isPressed || !_enableLongPress) return;
        
        _isPressed = true;
        _onLongPressStart.OnNext(Unit.Default);
        
        StartLongPressTimer();
    }
    
    /// <summary>
    /// Программно отменяет долгое нажатие
    /// </summary>
    public void CancelLongPress()
    {
        if (!_isPressed) return;
        
        _isPressed = false;
        _onLongPressEnd.OnNext(Unit.Default);
        StopLongPressTimer();
    }
    
    /// <summary>
    /// Перезапускает таймер долгого нажатия если нажатие активно
    /// </summary>
    public void RestartLongPress()
    {
        if (!_isPressed || !_enableLongPress) return;
        
        // Перезапускаем таймер без изменения состояния
        StartLongPressTimer();
    }
    
    /// <summary>
    /// Завершает текущее долгое нажатие и сбрасывает состояние для возможности начать новое
    /// </summary>
    public void CompleteAndReset()
    {
        if (_isPressed)
        {
            _isPressed = false;
            StopLongPressTimer();
        }
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Запускает таймер долгого нажатия
    /// </summary>
    private void StartLongPressTimer()
    {
        StopLongPressTimer();
        
        _longPressTimer = Observable.Timer(TimeSpan.FromSeconds(LongPressDuration))
            .Subscribe(_ =>
            {
                if (_isPressed)
                {
                    _onLongPressComplete.OnNext(Unit.Default);
                }
            });
    }
    
    /// <summary>
    /// Останавливает таймер долгого нажатия
    /// </summary>
    private void StopLongPressTimer()
    {
        _longPressTimer?.Dispose();
        _longPressTimer = null;
    }
    
    #endregion
}
