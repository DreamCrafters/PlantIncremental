using System;
using UniRx;
using UnityEngine;

/// <summary>
/// Сервис для управления визуальными эффектами полива
/// </summary>
public interface IWateringVisualizationService : IDisposable
{
    /// <summary>
    /// Состояние активности визуализации полива
    /// </summary>
    IReadOnlyReactiveProperty<bool> IsWateringVisualizationActive { get; }
    
    /// <summary>
    /// Текущая позиция курсора полива в мировых координатах
    /// </summary>
    IReadOnlyReactiveProperty<Vector3> WateringCursorWorldPosition { get; }
    
    /// <summary>
    /// Начинает визуализацию полива
    /// </summary>
    void StartWateringVisualization();
    
    /// <summary>
    /// Останавливает визуализацию полива
    /// </summary>
    void StopWateringVisualization();
    
    /// <summary>
    /// Обновляет позицию курсора полива
    /// </summary>
    /// <param name="worldPosition">Позиция в мировых координатах</param>
    void UpdateWateringCursorPosition(Vector3 worldPosition);
    
    /// <summary>
    /// События начала визуализации полива
    /// </summary>
    IObservable<Unit> OnWateringVisualizationStarted { get; }
    
    /// <summary>
    /// События окончания визуализации полива
    /// </summary>
    IObservable<Unit> OnWateringVisualizationStopped { get; }
}
