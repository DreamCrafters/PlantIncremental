using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Визуальное представление одной клетки игровой сетки
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GridCellView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private readonly Subject<Unit> _onClick = new();
    private readonly List<Tween> _activeTweens = new();

    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer _baseRenderer;
    [SerializeField] private SpriteRenderer _soilRenderer;
    [SerializeField] private SpriteRenderer _highlightRenderer;

    [Header("Sprites")]
    [SerializeField] private Sprite _fertileSprite;
    [SerializeField] private Sprite _rockySprite;
    [SerializeField] private Sprite _unsuitableSprite;

    [Header("Colors")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _highlightColor = new(1f, 1f, 0.8f, 0.5f);
    [SerializeField] private Color _unavailableColor = new(0.5f, 0.5f, 0.5f, 0.8f);

    [Header("Effects")]
    [SerializeField] private ParticleSystem _plantEffect;
    [SerializeField] private ParticleSystem _harvestEffect;

    // Состояние
    private PlantView _currentPlantView;
    private SoilType _currentSoilType;

    // Кэш
    private Vector3 _originalScale;

    public IObservable<Unit> OnClick => _onClick;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void OnDestroy()
    {
        KillAllTweens();
        _onClick?.Dispose();
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
        if (_highlightRenderer != null) DOTween.Kill(_highlightRenderer);
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
    /// Обработка клика по клетке
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
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
    }

    /// <summary>
    /// Обработка ухода курсора
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_baseRenderer == null || transform == null) return;

        _baseRenderer.color = _normalColor;
        
        var scaleTween = transform.DOScale(_originalScale, 0.1f)
            .SetTarget(transform);
        AddTween(scaleTween);

        // Убираем подсветку растения если оно есть
        if (_currentPlantView != null)
        {
            _currentPlantView.SetHighlight(false);
        }
    }

    /// <summary>
    /// Обновляет визуальное представление клетки
    /// </summary>
    public void UpdateVisual(GridCell cell)
    {
        if (cell == null) return;

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