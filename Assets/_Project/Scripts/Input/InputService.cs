using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using VContainer.Unity;

/// <summary>
/// Централизованный сервис для управления пользовательским вводом
/// </summary>
public class InputService : IInputService, ITickable, ILateTickable, IDisposable
{
    private readonly Camera _camera;
    private readonly CompositeDisposable _disposables = new();
    private readonly Dictionary<Vector2Int, LocalInputHandler> _cellHandlers = new();

    // Реактивные свойства глобального ввода
    private readonly ReactiveProperty<Vector2> _screenPosition = new(Vector2.zero);
    private readonly ReactiveProperty<Vector3> _worldPosition = new(Vector3.zero);
    private readonly ReactiveProperty<Vector2> _screenPositionLate = new(Vector2.zero);
    private readonly ReactiveProperty<Vector3> _worldPositionLate = new(Vector3.zero);

    // События глобального ввода
    private readonly Subject<Vector2> _onPointerMove = new();
    private readonly Subject<Vector2> _onPointerMoveLate = new();

    // Подписки на конкретные кнопки
    private readonly Dictionary<(KeyCode, InputTiming), Dictionary<int, Subject<Unit>>> _buttonDownSubscriptions = new();
    private readonly Dictionary<(KeyCode, InputTiming), Dictionary<int, Subject<Unit>>> _buttonUpSubscriptions = new();
    private readonly Dictionary<(KeyCode, InputTiming), Dictionary<int, ReactiveProperty<bool>>> _buttonStateSubscriptions = new();
    private readonly Dictionary<(KeyCode, InputTiming), bool> _buttonStates = new();
    private int _nextSubscriptionId = 0;

    // Подписки на кнопки для конкретных клеток
    private readonly Dictionary<(Vector2Int, KeyCode, InputTiming), Dictionary<int, Subject<Unit>>> _cellButtonDownSubscriptions = new();
    private readonly Dictionary<(Vector2Int, KeyCode, InputTiming), Dictionary<int, Subject<Unit>>> _cellButtonUpSubscriptions = new();
    private readonly Dictionary<(Vector2Int, KeyCode, InputTiming), Dictionary<int, ReactiveProperty<bool>>> _cellButtonStateSubscriptions = new();
    private readonly Dictionary<(Vector2Int, KeyCode, InputTiming), Dictionary<int, Subject<Unit>>> _cellButtonCompleteSubscriptions = new();
    private readonly Dictionary<int, float> _cellButtonCompleteDurations = new(); // subscriptionId -> duration
    private readonly Dictionary<int, float> _cellButtonCompleteLastFiredTime = new(); // subscriptionId -> last fire time
    private readonly Dictionary<(Vector2Int, KeyCode, InputTiming), bool> _cellButtonStates = new();
    private readonly Dictionary<(Vector2Int, KeyCode, InputTiming), float> _cellButtonPressStartTime = new();

    // Публичные свойства для раннего ввода (Tick)
    public IReadOnlyReactiveProperty<Vector2> ScreenPosition => _screenPosition;
    public IReadOnlyReactiveProperty<Vector3> WorldPosition => _worldPosition;
    public IObservable<Vector2> OnPointerMove => _onPointerMove;

    // Публичные свойства для позднего ввода (LateTick)
    public IReadOnlyReactiveProperty<Vector2> ScreenPositionLate => _screenPositionLate;
    public IReadOnlyReactiveProperty<Vector3> WorldPositionLate => _worldPositionLate;
    public IObservable<Vector2> OnPointerMoveLate => _onPointerMoveLate;

    public InputService()
    {
        _camera = Camera.main;

        // Инициализируем начальные значения
        UpdateInputState();
        UpdateLateInputState();

        // Настраиваем очистку ресурсов
        _screenPosition.AddTo(_disposables);
        _worldPosition.AddTo(_disposables);
        _onPointerMove.AddTo(_disposables);
        _screenPositionLate.AddTo(_disposables);
        _worldPositionLate.AddTo(_disposables);
        _onPointerMoveLate.AddTo(_disposables);
    }

    public void Tick()
    {
        UpdateInputState();
    }

    public void LateTick()
    {
        UpdateLateInputState();
    }

    public void Dispose()
    {
        _cellHandlers.Clear();

        // Очищаем подписки на кнопки
        _buttonDownSubscriptions.Clear();
        _buttonUpSubscriptions.Clear();
        _buttonStateSubscriptions.Clear();
        _buttonStates.Clear();

        // Очищаем подписки на кнопки для клеток
        _cellButtonDownSubscriptions.Clear();
        _cellButtonUpSubscriptions.Clear();
        _cellButtonStateSubscriptions.Clear();
        _cellButtonCompleteSubscriptions.Clear();
        _cellButtonCompleteDurations.Clear();
        _cellButtonCompleteLastFiredTime.Clear();
        _cellButtonStates.Clear();
        _cellButtonPressStartTime.Clear();

        _disposables?.Dispose();
    }

    /// <summary>
    /// Подписаться на нажатие конкретной кнопки
    /// </summary>
    /// <param name="keyCode">Код кнопки (клавиатура, мышь, джойстик)</param>
    /// <param name="timing">Время обработки (Tick или LateTick)</param>
    /// <returns>Observable для события нажатия</returns>
    public IObservable<Unit> SubscribeToButtonDown(KeyCode keyCode, InputTiming timing)
    {
        var key = (keyCode, timing);
        if (!_buttonDownSubscriptions.ContainsKey(key))
        {
            _buttonDownSubscriptions[key] = new Dictionary<int, Subject<Unit>>();
        }

        var subscriptionId = _nextSubscriptionId++;
        var subject = new Subject<Unit>();
        _buttonDownSubscriptions[key][subscriptionId] = subject;

        // Добавляем subject в disposables для автоматической очистки
        subject.AddTo(_disposables);

        return subject.AsObservable();
    }

    /// <summary>
    /// Подписаться на отпускание конкретной кнопки
    /// </summary>
    /// <param name="keyCode">Код кнопки (клавиатура, мышь, джойстик)</param>
    /// <param name="timing">Время обработки (Tick или LateTick)</param>
    /// <returns>Observable для события отпускания</returns>
    public IObservable<Unit> SubscribeToButtonUp(KeyCode keyCode, InputTiming timing)
    {
        var key = (keyCode, timing);
        if (!_buttonUpSubscriptions.ContainsKey(key))
        {
            _buttonUpSubscriptions[key] = new Dictionary<int, Subject<Unit>>();
        }

        var subscriptionId = _nextSubscriptionId++;
        var subject = new Subject<Unit>();
        _buttonUpSubscriptions[key][subscriptionId] = subject;

        // Добавляем subject в disposables для автоматической очистки
        subject.AddTo(_disposables);

        return subject.AsObservable();
    }

    /// <summary>
    /// Подписаться на состояние конкретной кнопки (нажата/не нажата)
    /// </summary>
    /// <param name="keyCode">Код кнопки (клавиатура, мышь, джойстик)</param>
    /// <param name="timing">Время обработки (Tick или LateTick)</param>
    /// <returns>ReactiveProperty с состоянием кнопки</returns>
    public IReadOnlyReactiveProperty<bool> SubscribeToButtonState(KeyCode keyCode, InputTiming timing)
    {
        var key = (keyCode, timing);
        if (!_buttonStateSubscriptions.ContainsKey(key))
        {
            _buttonStateSubscriptions[key] = new Dictionary<int, ReactiveProperty<bool>>();
        }

        var subscriptionId = _nextSubscriptionId++;
        var reactiveProperty = new ReactiveProperty<bool>(Input.GetKey(keyCode));
        _buttonStateSubscriptions[key][subscriptionId] = reactiveProperty;

        // Добавляем reactive property в disposables для автоматической очистки
        reactiveProperty.AddTo(_disposables);

        return reactiveProperty;
    }

    /// <summary>
    /// Подписаться на нажатие конкретной кнопки для конкретной клетки
    /// </summary>
    /// <param name="gridPosition">Позиция клетки на сетке</param>
    /// <param name="keyCode">Код кнопки (клавиатура, мышь, джойстик)</param>
    /// <param name="timing">Время обработки (Tick или LateTick)</param>
    /// <returns>Observable для события нажатия на клетку</returns>
    public IObservable<Unit> SubscribeToCellButtonDown(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing)
    {
        var key = (gridPosition, keyCode, timing);
        if (!_cellButtonDownSubscriptions.ContainsKey(key))
        {
            _cellButtonDownSubscriptions[key] = new Dictionary<int, Subject<Unit>>();
        }

        var subscriptionId = _nextSubscriptionId++;
        var subject = new Subject<Unit>();
        _cellButtonDownSubscriptions[key][subscriptionId] = subject;

        // Добавляем subject в disposables для автоматической очистки
        subject.AddTo(_disposables);

        return subject.AsObservable();
    }

    /// <summary>
    /// Подписаться на отпускание конкретной кнопки для конкретной клетки
    /// </summary>
    /// <param name="gridPosition">Позиция клетки на сетке</param>
    /// <param name="keyCode">Код кнопки (клавиатура, мышь, джойстик)</param>
    /// <param name="timing">Время обработки (Tick или LateTick)</param>
    /// <returns>Observable для события отпускания на клетку</returns>
    public IObservable<Unit> SubscribeToCellButtonUp(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing)
    {
        var key = (gridPosition, keyCode, timing);
        if (!_cellButtonUpSubscriptions.ContainsKey(key))
        {
            _cellButtonUpSubscriptions[key] = new Dictionary<int, Subject<Unit>>();
        }

        var subscriptionId = _nextSubscriptionId++;
        var subject = new Subject<Unit>();
        _cellButtonUpSubscriptions[key][subscriptionId] = subject;

        // Добавляем subject в disposables для автоматической очистки
        subject.AddTo(_disposables);

        return subject.AsObservable();
    }

    /// <summary>
    /// Подписаться на состояние конкретной кнопки для конкретной клетки
    /// </summary>
    /// <param name="gridPosition">Позиция клетки на сетке</param>
    /// <param name="keyCode">Код кнопки (клавиатура, мышь, джойстик)</param>
    /// <param name="timing">Время обработки (Tick или LateTick)</param>
    /// <returns>ReactiveProperty с состоянием кнопки для клетки</returns>
    public IReadOnlyReactiveProperty<bool> SubscribeToCellButtonState(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing)
    {
        var key = (gridPosition, keyCode, timing);
        if (!_cellButtonStateSubscriptions.ContainsKey(key))
        {
            _cellButtonStateSubscriptions[key] = new Dictionary<int, ReactiveProperty<bool>>();
        }

        var subscriptionId = _nextSubscriptionId++;
        var reactiveProperty = new ReactiveProperty<bool>(Input.GetKey(keyCode));
        _cellButtonStateSubscriptions[key][subscriptionId] = reactiveProperty;

        // Добавляем reactive property в disposables для автоматической очистки
        reactiveProperty.AddTo(_disposables);

        return reactiveProperty;
    }

    /// <summary>
    /// Подписаться на завершение длительного нажатия конкретной кнопки для конкретной клетки
    /// </summary>
    /// <param name="gridPosition">Позиция клетки на сетке</param>
    /// <param name="keyCode">Код кнопки (клавиатура, мышь, джойстик)</param>
    /// <param name="timing">Время обработки (Tick или LateTick)</param>
    /// <param name="longPressDuration">Длительность нажатия в секундах (по умолчанию 1.0)</param>
    /// <returns>Observable для события завершения длительного нажатия на клетку</returns>
    public IObservable<Unit> SubscribeToCellButtonComplete(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing, float longPressDuration = 1.0f)
    {
        var key = (gridPosition, keyCode, timing);
        if (!_cellButtonCompleteSubscriptions.ContainsKey(key))
        {
            _cellButtonCompleteSubscriptions[key] = new Dictionary<int, Subject<Unit>>();
        }

        var subscriptionId = _nextSubscriptionId++;
        var subject = new Subject<Unit>();
        _cellButtonCompleteSubscriptions[key][subscriptionId] = subject;
        
        // Сохраняем длительность для этой подписки
        _cellButtonCompleteDurations[subscriptionId] = longPressDuration;

        // Добавляем subject в disposables для автоматической очистки
        subject.AddTo(_disposables);

        return subject.AsObservable();
    }

    public void RegisterCellHandler(Vector2Int gridPosition, LocalInputHandler handler)
    {
        if (handler == null) return;

        // Отписываемся от старого обработчика если он есть
        if (_cellHandlers.TryGetValue(gridPosition, out var oldHandler))
        {
            UnsubscribeFromHandler(gridPosition, oldHandler);
        }

        // Регистрируем новый обработчик
        _cellHandlers[gridPosition] = handler;
    }

    public void UnregisterCellHandler(Vector2Int gridPosition)
    {
        if (_cellHandlers.TryGetValue(gridPosition, out var handler))
        {
            UnsubscribeFromHandler(gridPosition, handler);
            _cellHandlers.Remove(gridPosition);
        }
    }

    public LocalInputHandler GetCellHandler(Vector2Int gridPosition)
    {
        _cellHandlers.TryGetValue(gridPosition, out var handler);
        return handler;
    }

    private void UpdateInputState()
    {
        UpdateInputStateCore(
            _screenPosition, _worldPosition,
            _onPointerMove);

        // Обрабатываем подписки на кнопки для Tick
        UpdateButtonSubscriptions(InputTiming.Tick);
    }

    private void UpdateLateInputState()
    {
        UpdateInputStateCore(
            _screenPositionLate, _worldPositionLate,
            _onPointerMoveLate);

        // Обрабатываем подписки на кнопки для LateTick
        UpdateButtonSubscriptions(InputTiming.LateTick);
    }

    private void UpdateInputStateCore(
        ReactiveProperty<Vector2> screenPosition,
        ReactiveProperty<Vector3> worldPosition,
        Subject<Vector2> onPointerMove)
    {
        // Получаем текущую позицию мыши
        Vector2 currentScreenPos = Input.mousePosition;
        Vector3 currentWorldPos = _camera.ScreenToWorldPoint(new Vector3(currentScreenPos.x, currentScreenPos.y, _camera.nearClipPlane));
        currentWorldPos.z = 0;

        // Обновляем позиции если они изменились
        if (Vector2.Distance(screenPosition.Value, currentScreenPos) > 0.1f)
        {
            screenPosition.Value = currentScreenPos;
            worldPosition.Value = currentWorldPos;
            onPointerMove.OnNext(currentScreenPos);
        }
    }

    private void UpdateButtonSubscriptions(InputTiming timing)
    {
        // Получаем все уникальные кнопки для данного timing
        var buttonsToCheck = new HashSet<KeyCode>();

        // Собираем все кнопки из всех типов подписок
        foreach (var key in _buttonDownSubscriptions.Keys)
        {
            if (key.Item2 == timing) buttonsToCheck.Add(key.Item1);
        }
        foreach (var key in _buttonUpSubscriptions.Keys)
        {
            if (key.Item2 == timing) buttonsToCheck.Add(key.Item1);
        }
        foreach (var key in _buttonStateSubscriptions.Keys)
        {
            if (key.Item2 == timing) buttonsToCheck.Add(key.Item1);
        }

        // Собираем кнопки из подписок на клетки
        foreach (var key in _cellButtonDownSubscriptions.Keys)
        {
            if (key.Item3 == timing) buttonsToCheck.Add(key.Item2);
        }
        foreach (var key in _cellButtonUpSubscriptions.Keys)
        {
            if (key.Item3 == timing) buttonsToCheck.Add(key.Item2);
        }
        foreach (var key in _cellButtonStateSubscriptions.Keys)
        {
            if (key.Item3 == timing) buttonsToCheck.Add(key.Item2);
        }
        foreach (var key in _cellButtonCompleteSubscriptions.Keys)
        {
            if (key.Item3 == timing) buttonsToCheck.Add(key.Item2);
        }

        // Обрабатываем каждую кнопку
        foreach (var keyCode in buttonsToCheck)
        {
            var key = (keyCode, timing);
            bool isPressed = Input.GetKey(keyCode);
            bool wasPressed = _buttonStates.TryGetValue(key, out var previousState) ? previousState : false;

            // Обновляем состояние кнопки
            _buttonStates[key] = isPressed;

            // Обновляем ReactiveProperty для состояния кнопки
            if (_buttonStateSubscriptions.TryGetValue(key, out var stateSubscriptions))
            {
                foreach (var subscription in stateSubscriptions.Values)
                {
                    subscription.Value = isPressed;
                }
            }

            // Обрабатываем события нажатия
            if (isPressed && !wasPressed)
            {
                if (_buttonDownSubscriptions.TryGetValue(key, out var downSubscriptions))
                {
                    foreach (var subscription in downSubscriptions.Values)
                    {
                        subscription.OnNext(Unit.Default);
                    }
                }
            }

            // Обрабатываем события отпускания
            if (!isPressed && wasPressed)
            {
                if (_buttonUpSubscriptions.TryGetValue(key, out var upSubscriptions))
                {
                    foreach (var subscription in upSubscriptions.Values)
                    {
                        subscription.OnNext(Unit.Default);
                    }
                }
            }

            // Обрабатываем подписки на кнопки для клеток
            UpdateCellButtonSubscriptions(keyCode, timing, isPressed, wasPressed);
        }
    }

    private void UpdateCellButtonSubscriptions(KeyCode keyCode, InputTiming timing, bool isPressed, bool wasPressed)
    {
        // Обрабатываем события для всех зарегистрированных обработчиков клеток
        // События срабатывают только когда курсор находится над клеткой (handler.IsMouseOver)
        // Это обеспечивает точное попадание через Unity Event System вместо координатных расчетов
        foreach (var kvp in _cellHandlers)
        {
            var gridPosition = kvp.Key;
            var handler = kvp.Value;
            
            // Проверяем, что курсор находится над этой клеткой (через LocalInputHandler)
            if (!handler.IsMouseOver) continue;

            var cellKey = (gridPosition, keyCode, timing);
            
            // Обновляем состояние кнопки для клетки
            _cellButtonStates[cellKey] = isPressed;

            // Обновляем ReactiveProperty для состояния кнопки клетки
            if (_cellButtonStateSubscriptions.TryGetValue(cellKey, out var cellStateSubscriptions))
            {
                foreach (var subscription in cellStateSubscriptions.Values)
                {
                    subscription.Value = isPressed;
                }
            }

            // Обрабатываем события нажатия для клетки
            if (isPressed && !wasPressed)
            {
                // Запоминаем время начала нажатия
                _cellButtonPressStartTime[cellKey] = Time.time;
                
                if (_cellButtonDownSubscriptions.TryGetValue(cellKey, out var cellDownSubscriptions))
                {
                    foreach (var subscription in cellDownSubscriptions.Values)
                    {
                        subscription.OnNext(Unit.Default);
                    }
                }
            }

            // Обрабатываем события отпускания для клетки
            if (!isPressed && wasPressed)
            {
                // Убираем время нажатия при отпускании
                _cellButtonPressStartTime.Remove(cellKey);
                
                // Очищаем времена последнего срабатывания для всех подписок на complete события
                if (_cellButtonCompleteSubscriptions.TryGetValue(cellKey, out var cellCompleteSubscriptions))
                {
                    foreach (var subscriptionId in cellCompleteSubscriptions.Keys)
                    {
                        _cellButtonCompleteLastFiredTime.Remove(subscriptionId);
                    }
                }
                
                if (_cellButtonUpSubscriptions.TryGetValue(cellKey, out var cellUpSubscriptions))
                {
                    foreach (var subscription in cellUpSubscriptions.Values)
                    {
                        subscription.OnNext(Unit.Default);
                    }
                }
            }

            // Обрабатываем длительное нажатие
            if (isPressed && _cellButtonPressStartTime.TryGetValue(cellKey, out var pressStartTime))
            {
                float currentTime = Time.time;
                float pressDuration = currentTime - pressStartTime;
                
                // Проверяем каждую подписку на complete события для этой клетки
                if (_cellButtonCompleteSubscriptions.TryGetValue(cellKey, out var cellCompleteSubscriptions))
                {
                    foreach (var kvpSub in cellCompleteSubscriptions)
                    {
                        int subscriptionId = kvpSub.Key;
                        var subscription = kvpSub.Value;
                        
                        // Получаем длительность для этой конкретной подписки
                        if (_cellButtonCompleteDurations.TryGetValue(subscriptionId, out var requiredDuration))
                        {
                            // Проверяем, прошло ли достаточно времени с начала нажатия
                            if (pressDuration >= requiredDuration)
                            {
                                // Получаем время последнего срабатывания
                                bool shouldFire = false;
                                if (_cellButtonCompleteLastFiredTime.TryGetValue(subscriptionId, out var lastFireTime))
                                {
                                    // Проверяем, прошло ли достаточно времени с последнего срабатывания
                                    if (currentTime - lastFireTime >= requiredDuration)
                                    {
                                        shouldFire = true;
                                    }
                                }
                                else
                                {
                                    // Первое срабатывание для этой подписки
                                    shouldFire = true;
                                }
                                
                                if (shouldFire)
                                {
                                    subscription.OnNext(Unit.Default);
                                    _cellButtonCompleteLastFiredTime[subscriptionId] = currentTime;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void UnsubscribeFromHandler(Vector2Int gridPosition, LocalInputHandler handler)
    {
        // В данной реализации подписки автоматически очищаются через CompositeDisposable
        // При необходимости здесь можно добавить более специфичную логику отписки
    }
}