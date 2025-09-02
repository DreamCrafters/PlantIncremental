using System;
using UniRx;

/// <summary>
/// Сервис для работы со временем, обеспечивает абстракцию от Unity Time API
/// </summary>
public interface ITimeService
{
    /// <summary>
    /// Текущее время
    /// </summary>
    float CurrentTime { get; }
    
    /// <summary>
    /// Время с предыдущего кадра
    /// </summary>
    float DeltaTime { get; }
    
    /// <summary>
    /// Создает наблюдаемый таймер с указанным интервалом
    /// </summary>
    /// <param name="interval">Интервал таймера</param>
    /// <returns>Observable который срабатывает через указанный интервал</returns>
    IObservable<long> CreateTimer(TimeSpan interval);
    
    /// <summary>
    /// Создает наблюдаемый интервальный таймер
    /// </summary>
    /// <param name="interval">Интервал между срабатываниями</param>
    /// <returns>Observable который срабатывает каждый интервал</returns>
    IObservable<long> CreateInterval(TimeSpan interval);
}