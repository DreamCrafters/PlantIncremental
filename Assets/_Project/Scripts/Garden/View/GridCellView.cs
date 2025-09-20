using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Визуальное представление одной клетки игровой сетки с поддержкой полива
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GridCellView : MonoBehaviour
{
    private const float CellMoveHeight = 0.05f;
    private const float CellMoveDuration = 0.1f;

    private readonly List<Tween> _activeTweens = new();

    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer _baseRenderer;
    [SerializeField] private SpriteRenderer _soilRenderer;
    [SerializeField] private Transform _visualsParent;

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
    private LocalInputHandler _inputHandler;

    // Кэш
    private Vector3 _originalScale;
    private Vector3 _originalPosition;

    private void Awake()
    {
        _originalScale = _visualsParent.localScale;

        // Добавляем компонент для обработки ввода
        if (gameObject.TryGetComponent(out _inputHandler) == false)
        {
            _inputHandler = gameObject.AddComponent<LocalInputHandler>();
        }

        // Подписываемся на события ввода
        _inputHandler.OnPointerEntered.Subscribe(OnCellPointerEnter).AddTo(gameObject);
        _inputHandler.OnPointerExited.Subscribe(OnCellPointerExit).AddTo(gameObject);
    }

    private void Start()
    {
        _originalPosition = _visualsParent.localPosition;
    }

    private void OnDestroy()
    {
        KillAllTweens();
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
            plantView.transform.SetParent(_visualsParent);
            plantView.transform.localPosition = Vector3.zero;
        }
    }

    /// <summary>
    /// Устанавливает сущность растения для клетки
    /// </summary>
    public void SetPlantEntity(PlantEntity plantEntity)
    {
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

        if (_visualsParent == null || _baseRenderer == null) return;

        // Анимация клетки при посадке
        var sequence = DOTween.Sequence();
        sequence.SetTarget(_visualsParent);

        var scale1 = _visualsParent.DOScale(_originalScale * 0.9f, 0.1f).SetTarget(_visualsParent);
        var scale2 = _visualsParent.DOScale(_originalScale * 1.1f, 0.1f).SetTarget(_visualsParent);
        var scale3 = _visualsParent.DOScale(_originalScale, 0.1f).SetTarget(_visualsParent);

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

        if (_visualsParent == null || _baseRenderer == null) return;

        // Анимация клетки при сборе
        var sequence = DOTween.Sequence();
        sequence.SetTarget(_visualsParent);

        var punchTween = _visualsParent.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1f)
            .SetTarget(_visualsParent);
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
    }

    #region Input Event Handlers

    /// <summary>
    /// Обработка наведения курсора
    /// </summary>
    private void OnCellPointerEnter(PointerEventData eventData)
    {
        if (_visualsParent == null) return;

        // Поднимаем клетку к фиксированной позиции
        var targetPosition = new Vector3(_originalPosition.x, _originalPosition.y + CellMoveHeight, _originalPosition.z);
        var moveTween = _visualsParent.DOLocalMove(targetPosition, CellMoveDuration)
            .SetTarget(_visualsParent);
        AddTween(moveTween);
    }

    /// <summary>
    /// Обработка ухода курсора
    /// </summary>
    private void OnCellPointerExit(PointerEventData eventData)
    {
        if (_visualsParent == null) return;

        // Опускаем клетку к исходной позиции
        var moveTween = _visualsParent.DOLocalMove(_originalPosition, 0.1f)
            .SetTarget(_visualsParent);
        AddTween(moveTween);
    }

    #endregion

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
        DOTween.Kill(_visualsParent);
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
}