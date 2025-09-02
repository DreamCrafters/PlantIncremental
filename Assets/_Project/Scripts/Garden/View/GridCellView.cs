using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Визуальное представление одной клетки игровой сетки с поддержкой полива
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GridCellView : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    private readonly Subject<Unit> _onClick = new();
    private readonly Subject<IPlantEntity> _onWateringStart = new();
    private readonly Subject<IPlantEntity> _onWateringEnd = new();
    private readonly Subject<IPlantEntity> _onWateringComplete = new();
    private readonly List<Tween> _activeTweens = new();

    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer _baseRenderer;
    [SerializeField] private SpriteRenderer _soilRenderer;

    [Header("Sprites")]
    [SerializeField] private Sprite _fertileSprite;
    [SerializeField] private Sprite _rockySprite;
    [SerializeField] private Sprite _unsuitableSprite;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _highlightColor = new(1f, 1f, 0.8f, 0.5f);

    [Header("Effects")]
    [SerializeField] private ParticleSystem _plantEffect;
    [SerializeField] private ParticleSystem _harvestEffect;
    [SerializeField] private ParticleSystem _destroyEffect;

    // Состояние
    private PlantView _currentPlantView;
    private SoilType _currentSoilType;
    private IPlantEntity _currentPlantEntity;
    private LongPressHandler _longPressHandler;
    private bool _isMouseOver;
    private bool _isDragging;

    // Кэш
    private Vector3 _originalScale;

    public IObservable<Unit> OnClick => _onClick;
    public IObservable<IPlantEntity> OnWateringStart => _onWateringStart;
    public IObservable<IPlantEntity> OnWateringEnd => _onWateringEnd;
    public IObservable<IPlantEntity> OnWateringComplete => _onWateringComplete;

    private void Awake()
    {
        _originalScale = transform.localScale;
        
        // Добавляем компонент для обработки долгих нажатий
        if (gameObject.TryGetComponent(out _longPressHandler) == false)
        {
            _longPressHandler = gameObject.AddComponent<LongPressHandler>();
        }
        
        // Подписываемся на события долгого нажатия
        _longPressHandler.OnLongPressStart.Subscribe(_ => OnLongPressStart()).AddTo(gameObject);
        _longPressHandler.OnLongPressEnd.Subscribe(_ => OnLongPressEnd()).AddTo(gameObject);
        _longPressHandler.OnLongPressComplete.Subscribe(_ => OnLongPressComplete()).AddTo(gameObject);
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
            if (_longPressHandler != null && _longPressHandler.IsPressed)
            {
                var fakeEventData = new PointerEventData(EventSystem.current)
                {
                    button = PointerEventData.InputButton.Left,
                    position = Input.mousePosition
                };
                _longPressHandler.OnPointerUp(fakeEventData);
            }
        }
        
        // Автоматический запуск поливки при drag и доступном для поливки растении
        if (_isDragging && _isMouseOver && _currentPlantEntity != null && 
            _currentPlantEntity.IsWaitingForWater && _longPressHandler != null && !_longPressHandler.IsPressed)
        {
            _longPressHandler.StartLongPress();
        }
    }

    private void OnDestroy()
    {
        KillAllTweens();
        _onClick?.Dispose();
        _onWateringStart?.Dispose();
        _onWateringEnd?.Dispose();
        _onWateringComplete?.Dispose();
    }

    /// <summary>
    /// Обработка клика по клетке
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _onClick.OnNext(Unit.Default);
        }
    }

    /// <summary>
    /// Обработка наведения курсора
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_baseRenderer == null || transform == null) return;

        _isMouseOver = true;

        // Легкая подсветка при наведении
        _baseRenderer.color = Color.Lerp(_normalColor, _highlightColor, 0.3f);

        var scaleTween = transform.DOScale(_originalScale * 1.02f, 0.1f)
            .SetTarget(transform);
        AddTween(scaleTween);

        // Подсвечиваем растение если оно есть
        if (_currentPlantView != null)
        {
            _currentPlantView.SetHighlight(true);
        }
        
        // Если есть растение которое можно поливать и долгое нажатие активно - перезапускаем таймер
        if (_currentPlantEntity != null && _currentPlantEntity.IsWaitingForWater && _longPressHandler != null)
        {
            _longPressHandler.RestartLongPress();
        }
        
        // Поддержка drag-to-water обрабатывается в Update
    }

    /// <summary>
    /// Обработка ухода курсора
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_baseRenderer == null || transform == null) return;

        _isMouseOver = false;
        _baseRenderer.color = _normalColor;

        var scaleTween = transform.DOScale(_originalScale, 0.1f)
            .SetTarget(transform);
        AddTween(scaleTween);

        // Убираем подсветку растения если оно есть
        if (_currentPlantView != null)
        {
            _currentPlantView.SetHighlight(false);
        }
        
        // При уходе курсора во время drag - отменяем долгое нажатие
        if (_longPressHandler != null && _longPressHandler.IsPressed && !Input.GetMouseButton(0))
        {
            _longPressHandler.CancelLongPress();
        }
    }

    /// <summary>
    /// Обработка начала долгого нажатия
    /// </summary>
    private void OnLongPressStart()
    {
        if (_currentPlantEntity != null && _currentPlantEntity.IsWaitingForWater)
        {
            _onWateringStart.OnNext(_currentPlantEntity);
        }
    }

    /// <summary>
    /// Обработка окончания долгого нажатия (отпущена кнопка)
    /// </summary>
    private void OnLongPressEnd()
    {
        if (_currentPlantEntity != null)
        {
            _onWateringEnd.OnNext(_currentPlantEntity);
        }
    }

    /// <summary>
    /// Обработка завершения долгого нажатия (прошло нужное время)
    /// </summary>
    private void OnLongPressComplete()
    {
        if (_currentPlantEntity != null && _currentPlantEntity.IsWaitingForWater)
        {
            _onWateringComplete.OnNext(_currentPlantEntity);
            
            // Сбрасываем состояние LongPressHandler для возможности начать новую поливку
            if (_longPressHandler != null)
            {
                _longPressHandler.CompleteAndReset();
            }
        }
    }

    /// <summary>
    /// Безопасно убивает все активные твины
    /// </summary>
    private void KillAllTweens()
    {
        // Отменяем все активные твины из списка
        for (int i = _activeTweens.Count - 1; i >= 0; i--)
        {
            if (_activeTweens[i] != null && _activeTweens[i].IsActive())
            {
                _activeTweens[i].Kill();
            }
        }
        _activeTweens.Clear();

        // Дополнительная очистка по объектам
        DOTween.Kill(transform);
        if (_baseRenderer != null) DOTween.Kill(_baseRenderer);
        if (_soilRenderer != null) DOTween.Kill(_soilRenderer);
    }

    /// <summary>
    /// Добавляет твин в список активных для отслеживания
    /// </summary>
    private void AddTween(Tween tween)
    {
        if (tween != null)
        {
            _activeTweens.Add(tween);

            // Автоматически удаляем из списка по завершении
            tween.OnComplete(() =>
            {
                if (tween != null)
                {
                    _activeTweens.Remove(tween);
                }
            });

            // Дополнительная безопасность - удаляем из списка при убийстве
            tween.OnKill(() =>
            {
                if (tween != null)
                {
                    _activeTweens.Remove(tween);
                }
            });
        }
    }

    /// <summary>
    /// Обновляет визуальное представление клетки
    /// </summary>
    public void UpdateVisual(GridCell cell)
    {
        if (cell == null) return;

        SetSoilType(cell.SoilType);

        _baseRenderer.color = _normalColor;
    }

    /// <summary>
    /// Показывает или скрывает растение на клетке
    /// </summary>
    public void ShowPlant(bool show)
    {
        if (show == false && _currentPlantView != null)
        {
            _currentPlantView = null;
        }
    }

    /// <summary>
    /// Устанавливает растение на клетку
    /// </summary>
    public void SetPlantView(PlantView plantView)
    {
        _currentPlantView = plantView;
        if (plantView != null)
        {
            plantView.transform.SetParent(transform);
            plantView.transform.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// Устанавливает сущность растения для клетки
    /// </summary>
    public void SetPlantEntity(IPlantEntity plantEntity)
    {
        _currentPlantEntity = plantEntity;
        if (plantEntity != null)
        {
            SetPlantView(plantEntity.View);
        }
        else
        {
            SetPlantView(null);
        }
    }

    /// <summary>
    /// Воспроизводит эффект посадки растения
    /// </summary>
    public void PlayPlantEffect()
    {
        if (_plantEffect != null)
        {
            _plantEffect.Play();
        }

        if (transform == null || _baseRenderer == null) return;

        // Анимация клетки при посадке
        var sequence = DOTween.Sequence();
        sequence.SetTarget(transform);

        var scale1 = transform.DOScale(_originalScale * 0.9f, 0.1f).SetTarget(transform);
        var scale2 = transform.DOScale(_originalScale * 1.1f, 0.1f).SetTarget(transform);
        var scale3 = transform.DOScale(_originalScale, 0.1f).SetTarget(transform);

        sequence.Append(scale1);
        sequence.Append(scale2);
        sequence.Append(scale3);

        // Вспышка цвета
        var colorTween1 = _baseRenderer.DOColor(Color.white, 0.1f).SetTarget(_baseRenderer);
        var colorTween2 = _baseRenderer.DOColor(_normalColor, 0.2f).SetTarget(_baseRenderer);

        sequence.Join(colorTween1);
        sequence.Append(colorTween2);

        AddTween(sequence);
    }

    /// <summary>
    /// Воспроизводит эффект сбора урожая
    /// </summary>
    public void PlayHarvestEffect()
    {
        if (_harvestEffect != null)
        {
            _harvestEffect.Play();
        }

        if (transform == null || _baseRenderer == null) return;

        // Анимация клетки при сборе
        var sequence = DOTween.Sequence();
        sequence.SetTarget(transform);

        var punchTween = transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1f)
            .SetTarget(transform);
        sequence.Append(punchTween);

        // Вспышка радости
        var colorTween1 = _baseRenderer.DOColor(new Color(1f, 1f, 0.5f, 1f), 0.1f)
            .SetTarget(_baseRenderer);
        var colorTween2 = _baseRenderer.DOColor(_normalColor, 0.2f)
            .SetTarget(_baseRenderer);

        sequence.Join(colorTween1);
        sequence.Append(colorTween2);

        AddTween(sequence);
    }

    /// <summary>
    /// Воспроизводит эффект уничтожения увядшего растения
    /// </summary>
    public void PlayDestroyEffect()
    {
        if (_destroyEffect != null)
        {
            _destroyEffect.Play();
        }

        if (transform == null || _baseRenderer == null) return;

        // Анимация клетки при уничтожении
        var sequence = DOTween.Sequence();
        sequence.SetTarget(transform);

        // Дрожание клетки
        var shakeTween = transform.DOShakePosition(0.4f, strength: 0.01f, vibrato: 15)
            .SetTarget(transform);
        sequence.Append(shakeTween);

        AddTween(sequence);
    }

    /// <summary>
    /// Устанавливает тип почвы для визуализации
    /// </summary>
    private void SetSoilType(SoilType soilType)
    {
        if (_soilRenderer == null) return;

        _soilRenderer.sprite = soilType switch
        {
            SoilType.Fertile => _fertileSprite,
            SoilType.Rocky => _rockySprite,
            SoilType.Unsuitable => _unsuitableSprite,
            _ => null
        };

        if (_currentSoilType != soilType && _soilRenderer.transform != null)
        {
            var sequence = DOTween.Sequence();
            sequence.SetTarget(_soilRenderer.transform);

            var scale1 = _soilRenderer.transform.DOScale(1.1f, 0.2f)
                .SetTarget(_soilRenderer.transform);
            var scale2 = _soilRenderer.transform.DOScale(1f, 0.1f)
                .SetTarget(_soilRenderer.transform);

            sequence.Append(scale1);
            sequence.Append(scale2);

            AddTween(sequence);
        }

        _currentSoilType = soilType;
    }
}