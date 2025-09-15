using System;
using UniRx;
using UnityEngine;

/// <summary>
/// Сервис для централизованной обработки пользовательского ввода
/// </summary>
public interface IInputService
{
    // Реактивные свойства глобального состояния
    IReadOnlyReactiveProperty<Vector2> ScreenPosition { get; }
    IReadOnlyReactiveProperty<Vector3> WorldPosition { get; }
    IReadOnlyReactiveProperty<Vector2> ScreenPositionLate { get; }
    IReadOnlyReactiveProperty<Vector3> WorldPositionLate { get; }

    // Глобальные события ввода
    IObservable<Vector2> OnPointerMove { get; }
    IObservable<Vector2> OnPointerMoveLate { get; }

    IObservable<Unit> SubscribeToButtonDown(KeyCode keyCode, InputTiming timing);
    IObservable<Unit> SubscribeToButtonUp(KeyCode keyCode, InputTiming timing);
    IReadOnlyReactiveProperty<bool> SubscribeToButtonState(KeyCode keyCode, InputTiming timing);

    // Подписки на кнопки для конкретных клеток
    IObservable<Unit> SubscribeToCellButtonDown(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing);
    IObservable<Unit> SubscribeToCellButtonUp(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing);
    IReadOnlyReactiveProperty<bool> SubscribeToCellButtonState(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing);
    IObservable<Unit> SubscribeToCellButtonComplete(Vector2Int gridPosition, KeyCode keyCode, InputTiming timing, float longPressDuration = 1.0f);

    // Методы для перезапуска таймеров
    void RestartCellButtonTimer(IPlantEntity plant);

    // Методы для регистрации локальных обработчиков ввода
    void RegisterCellHandler(Vector2Int gridPosition, LocalInputHandler handler);
    void UnregisterCellHandler(Vector2Int gridPosition);

    // Получение обработчика для конкретной клетки
    LocalInputHandler GetCellHandler(Vector2Int gridPosition);
}