using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Универсальный компонент для обработки локального ввода - кликов, наведения курсора, долгих нажатий и перетаскивания
/// </summary>
public class LocalInputHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // События кликов и наведения
    private readonly Subject<PointerEventData> _onPointerEnter = new();
    private readonly Subject<PointerEventData> _onPointerExit = new();
    
    // Состояние взаимодействия
    private bool _isMouseOver;

    // Публичные события
    public IObservable<PointerEventData> OnPointerEntered => _onPointerEnter;
    public IObservable<PointerEventData> OnPointerExited => _onPointerExit;
    
    public bool IsMouseOver => _isMouseOver;

    private void OnDestroy()
    {
        _onPointerEnter?.Dispose();
        _onPointerExit?.Dispose();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _isMouseOver = true;
        _onPointerEnter.OnNext(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isMouseOver = false;
        _onPointerExit.OnNext(eventData);
    }
}
