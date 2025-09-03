using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using VContainer;

/// <summary>
/// Компонент для управления визуальными эффектами полива
/// </summary>
public class WateringEffectsView : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private List<GameObject> _wateringEffects = new();
    [SerializeField] private bool _enableOnStart = true;
    [SerializeField] private bool _disableOnStop = true;
    [SerializeField] private float _effectDelay = 0f;
    
    [Header("Position Settings")]
    [SerializeField] private bool _followCursorPosition = false;
    [SerializeField] private Vector3 _effectOffset = Vector3.zero;
    
    private IWateringVisualizationService _wateringVisualizationService;
    private CompositeDisposable _disposables = new();
    private List<IDisposable> _delayedEffectDisposables = new();
    
    [Inject]
    public void Construct(IWateringVisualizationService wateringVisualizationService)
    {
        _wateringVisualizationService = wateringVisualizationService;
    }
    
    private void Start()
    {
        // Изначально отключаем все эффекты
        SetEffectsActive(false);
        
        // Подписываемся на события начала и окончания визуализации полива
        _wateringVisualizationService.OnWateringVisualizationStarted
            .Subscribe(_ => OnWateringStarted())
            .AddTo(_disposables);
            
        _wateringVisualizationService.OnWateringVisualizationStopped
            .Subscribe(_ => OnWateringStopped())
            .AddTo(_disposables);
        
        // Если нужно следовать за курсором, подписываемся на изменения позиции
        if (_followCursorPosition)
        {
            _wateringVisualizationService.WateringCursorWorldPosition
                .Where(_ => _wateringVisualizationService.IsWateringVisualizationActive.Value)
                .Subscribe(worldPos => UpdateEffectsPosition(worldPos))
                .AddTo(_disposables);
        }
    }
    
    private void OnWateringStarted()
    {
        if (_enableOnStart)
        {
            if (_effectDelay > 0)
            {
                // Включаем эффекты с задержкой
                var delayedDisposable = Observable.Timer(System.TimeSpan.FromSeconds(_effectDelay))
                    .Subscribe(_ => SetEffectsActive(true));
                    
                _delayedEffectDisposables.Add(delayedDisposable);
            }
            else
            {
                // Включаем эффекты сразу
                SetEffectsActive(true);
            }
        }
    }
    
    private void OnWateringStopped()
    {
        // Отменяем все отложенные активации
        ClearDelayedEffects();
        
        if (_disableOnStop)
        {
            SetEffectsActive(false);
        }
    }
    
    private void SetEffectsActive(bool active)
    {
        foreach (var effect in _wateringEffects)
        {
            if (effect != null)
            {
                effect.SetActive(active);
            }
        }
    }
    
    private void UpdateEffectsPosition(Vector3 worldPosition)
    {
        transform.position = worldPosition + _effectOffset;
    }
    
    private void ClearDelayedEffects()
    {
        foreach (var disposable in _delayedEffectDisposables)
        {
            disposable?.Dispose();
        }
        _delayedEffectDisposables.Clear();
    }
    
    private void OnDestroy()
    {
        ClearDelayedEffects();
        _disposables?.Dispose();
    }
    
    // Методы для настройки через код
    public void AddEffect(GameObject effect)
    {
        if (effect != null && !_wateringEffects.Contains(effect))
        {
            _wateringEffects.Add(effect);
        }
    }
    
    public void RemoveEffect(GameObject effect)
    {
        _wateringEffects.Remove(effect);
    }
    
    public void ClearEffects()
    {
        _wateringEffects.Clear();
    }
    
    public void SetEffectDelay(float delay)
    {
        _effectDelay = Mathf.Max(0f, delay);
    }
    
    public void SetFollowCursorPosition(bool follow)
    {
        _followCursorPosition = follow;
    }
    
    public void SetEffectOffset(Vector3 offset)
    {
        _effectOffset = offset;
    }
}
