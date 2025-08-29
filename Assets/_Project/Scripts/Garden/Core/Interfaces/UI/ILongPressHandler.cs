using System;
using UniRx;

/// <summary>
/// Интерфейс для обработки долгих нажатий на UI элементах
/// </summary>
public interface ILongPressHandler
{
    /// <summary>
    /// Событие начала долгого нажатия
    /// </summary>
    IObservable<Unit> OnLongPressStart { get; }
    
    /// <summary>
    /// Событие завершения долгого нажатия
    /// </summary>
    IObservable<Unit> OnLongPressEnd { get; }
    
    /// <summary>
    /// Событие успешного долгого нажатия (прошла необходимая длительность)
    /// </summary>
    IObservable<Unit> OnLongPressComplete { get; }
}
