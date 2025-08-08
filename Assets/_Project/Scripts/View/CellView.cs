using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Визуальное представление одной клетки игровой сетки
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class CellView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer _baseRenderer;
    [SerializeField] private SpriteRenderer _soilRenderer;
    [SerializeField] private SpriteRenderer _highlightRenderer;
    [SerializeField] private GameObject _harvestIndicator;

    [Header("Sprites")]
    [SerializeField] private Sprite _fertileSprite;
    [SerializeField] private Sprite _rockySprite;
    [SerializeField] private Sprite _unsuitableSprite;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _highlightColor = new Color(1f, 1f, 0.8f, 0.5f);
    [SerializeField] private Color _unavailableColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

    [Header("Effects")]
    [SerializeField] private ParticleSystem _plantEffect;
    [SerializeField] private ParticleSystem _harvestEffect;
    [SerializeField] private GameObject _glowEffect;

    // События
    private readonly Subject<Unit> _onClick = new();
    public IObservable<Unit> OnClick => _onClick;

    // Состояние
    private Vector2Int _gridPosition;
    private SoilType _currentSoilType;
    private bool _isHighlighted;
    private bool _isOccupied;
    private bool _isHarvestReady;
    private PlantView _currentPlantView;

    // Кэш
    private Vector3 _originalScale;
    private BoxCollider2D _collider;

    private void Awake()
    {
        if (TryGetComponent(out _collider) == false)
        {
            _collider = gameObject.AddComponent<BoxCollider2D>();
        }

        _originalScale = transform.localScale;
    }

    private void OnDestroy()
    {
        DOTween.Kill(transform);
        DOTween.Kill(_baseRenderer);
        DOTween.Kill(_soilRenderer);
        DOTween.Kill(_highlightRenderer);

        if (_harvestIndicator != null)
        {
            DOTween.Kill(_harvestIndicator.transform);
        }

        _onClick?.Dispose();
    }

    /// <summary>
    /// Обработка клика по клетке
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            _onClick.OnNext(Unit.Default);

            // Визуальный отклик на клик
            transform.DOScale(_originalScale * 0.95f, 0.05f)
                .OnComplete(() => transform.DOScale(_originalScale, 0.05f));
        }
    }

    /// <summary>
    /// Обработка наведения курсора
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isHighlighted == false)
        {
            // Легкая подсветка при наведении
            _baseRenderer.color = Color.Lerp(_normalColor, _highlightColor, 0.3f);
            transform.DOScale(_originalScale * 1.02f, 0.1f);
        }
    }

    /// <summary>
    /// Обработка ухода курсора
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isHighlighted == false)
        {
            _baseRenderer.color = _normalColor;
            transform.DOScale(_originalScale, 0.1f);
        }
    }

    /// <summary>
    /// Инициализирует клетку с параметрами
    /// </summary>
    public void Initialize(Vector2Int gridPosition, float cellSize)
    {
        _gridPosition = gridPosition;
        _collider.size = Vector2.one * cellSize;
        transform.localScale = Vector3.one * cellSize;
    }

    /// <summary>
    /// Обновляет визуальное представление клетки
    /// </summary>
    public void UpdateVisual(GridCell cell)
    {
        if (cell == null) return;

        _isOccupied = !cell.IsEmpty;
        SetSoilType(cell.SoilType);

        // Обновляем цвет в зависимости от состояния
        if (cell.SoilType == SoilType.Unsuitable)
        {
            _baseRenderer.color = _unavailableColor;
        }
        else
        {
            _baseRenderer.color = _normalColor;
        }
    }

    /// <summary>
    /// Показывает или скрывает растение на клетке
    /// </summary>
    public void ShowPlant(bool show)
    {
        _isOccupied = show;

        if (show == false && _currentPlantView != null)
        {
            Destroy(_currentPlantView.gameObject);
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
    /// Включает/выключает подсветку клетки
    /// </summary>
    public void SetHighlight(bool active)
    {
        if (_isHighlighted == active) return;

        _isHighlighted = active;
        _highlightRenderer.enabled = active;

        if (active)
        {
            // Анимация подсветки
            _highlightRenderer.color = new Color(_highlightColor.r, _highlightColor.g, _highlightColor.b, 0);
            _highlightRenderer.DOFade(_highlightColor.a, 0.2f);

            transform.DOScale(_originalScale * 1.05f, 0.1f)
                .SetEase(Ease.OutBack);

            // Эффект свечения
            if (_glowEffect != null)
            {
                _glowEffect.SetActive(true);
            }
        }
        else
        {
            _highlightRenderer.DOFade(0, 0.2f)
                .OnComplete(() => _highlightRenderer.enabled = false);

            transform.DOScale(_originalScale, 0.1f)
                .SetEase(Ease.OutBack);

            if (_glowEffect != null)
            {
                _glowEffect.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Показывает индикатор готовности к сбору
    /// </summary>
    public void ShowHarvestReady(bool show)
    {
        _isHarvestReady = show;

        if (_harvestIndicator != null)
        {
            _harvestIndicator.SetActive(show);

            if (show)
            {
                // Анимация появления индикатора
                _harvestIndicator.transform.localScale = Vector3.zero;
                _harvestIndicator.transform.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack);

                // Пульсирующая анимация
                _harvestIndicator.transform.DOScale(Vector3.one * 1.1f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
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

        // Анимация клетки при посадке
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOScale(_originalScale * 0.9f, 0.1f));
        sequence.Append(transform.DOScale(_originalScale * 1.1f, 0.1f));
        sequence.Append(transform.DOScale(_originalScale, 0.1f));

        // Вспышка цвета
        sequence.Join(_baseRenderer.DOColor(Color.white, 0.1f));
        sequence.Append(_baseRenderer.DOColor(_normalColor, 0.2f));
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

        // Скрываем индикатор сбора
        ShowHarvestReady(false);

        // Анимация клетки при сборе
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1f));

        // Вспышка радости
        sequence.Join(_baseRenderer.DOColor(new Color(1f, 1f, 0.5f, 1f), 0.1f));
        sequence.Append(_baseRenderer.DOColor(_normalColor, 0.2f));
    }

    /// <summary>
    /// Анимация недоступности клетки
    /// </summary>
    public void ShowUnavailable()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(transform.DOShakeRotation(0.2f, 5f, 10));
        sequence.Join(_baseRenderer.DOColor(Color.red, 0.1f));
        sequence.Append(_baseRenderer.DOColor(_normalColor, 0.1f));
    }

    /// <summary>
    /// Анимация улучшения почвы
    /// </summary>
    public void PlaySoilImproveEffect()
    {
        var sequence = DOTween.Sequence();

        // Волна улучшения
        sequence.Append(transform.DOScale(_originalScale * 1.2f, 0.3f).SetEase(Ease.OutBack));
        sequence.Join(_soilRenderer.DOColor(Color.green, 0.3f));
        sequence.Append(transform.DOScale(_originalScale, 0.2f));
        sequence.Append(_soilRenderer.DOColor(GetSoilColor(_currentSoilType), 0.2f));

        // Частицы улучшения
        if (_plantEffect != null)
        {
            _plantEffect.Play();
        }
    }

    /// <summary>
    /// Получает позицию клетки в сетке
    /// </summary>
    public Vector2Int GetGridPosition()
    {
        return _gridPosition;
    }

    /// <summary>
    /// Проверяет, занята ли клетка
    /// </summary>
    public bool IsOccupied()
    {
        return _isOccupied;
    }

    /// <summary>
    /// Проверяет, готова ли клетка к сбору
    /// </summary>
    public bool IsReadyToHarvest()
    {
        return _isHarvestReady;
    }

    /// <summary>
    /// Устанавливает тип почвы для визуализации
    /// </summary>
    private void SetSoilType(SoilType soilType)
    {
        _currentSoilType = soilType;

        // Обновляем спрайт почвы
        _soilRenderer.sprite = soilType switch
        {
            SoilType.Fertile => _fertileSprite,
            SoilType.Rocky => _rockySprite,
            SoilType.Unsuitable => _unsuitableSprite,
            _ => null
        };

        // Обновляем цвет почвы
        _soilRenderer.color = GetSoilColor(soilType);

        // Анимация смены типа почвы
        if (_soilRenderer.sprite != null)
        {
            _soilRenderer.transform.DOScale(1.1f, 0.2f)
                .OnComplete(() => _soilRenderer.transform.DOScale(1f, 0.1f));
        }
    }

    private Color GetSoilColor(SoilType soilType)
    {
        return soilType switch
        {
            SoilType.Fertile => new Color(0.4f, 0.8f, 0.3f, 0.5f),
            SoilType.Rocky => new Color(0.6f, 0.5f, 0.4f, 0.5f),
            SoilType.Unsuitable => new Color(0.3f, 0.2f, 0.2f, 0.5f),
            _ => Color.white
        };
    }
}