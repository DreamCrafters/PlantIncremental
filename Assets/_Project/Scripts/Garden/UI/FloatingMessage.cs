using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Всплывающее сообщение
/// </summary>
public class FloatingMessage : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Image _background;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Colors")]
    [SerializeField] private Color _infoColor = new(0.2f, 0.5f, 1f);
    [SerializeField] private Color _successColor = new(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color _warningColor = new(1f, 0.7f, 0.2f);
    [SerializeField] private Color _errorColor = new(1f, 0.2f, 0.2f);

    [Header("Animation")]
    [SerializeField] private float _displayDuration = 2f;
    [SerializeField] private float _fadeInDuration = 0.3f;
    [SerializeField] private float _fadeOutDuration = 0.5f;

    public event Action OnComplete;

    // Отслеживание твинов
    private readonly List<Tween> _activeTweens = new();

    private void Awake()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (_background == null)
            _background = GetComponent<Image>();
    }

    private void OnDestroy()
    {
        KillAllTweens();
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
        if (_canvasGroup != null) DOTween.Kill(_canvasGroup);
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
                    OnComplete?.Invoke();
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
    /// Показывает сообщение
    /// </summary>
    public void Show(string message, MessageType type)
    {
        _messageText.text = message;
        _background.color = GetColorForType(type);

        gameObject.SetActive(true);
        AnimateMessage();
    }

    private Color GetColorForType(MessageType type)
    {
        return type switch
        {
            MessageType.Info => _infoColor,
            MessageType.Success => _successColor,
            MessageType.Warning => _warningColor,
            MessageType.Error => _errorColor,
            _ => _infoColor
        };
    }

    private void AnimateMessage()
    {
        if (_canvasGroup == null || transform == null) return;

        _canvasGroup.alpha = 0;
        transform.localScale = Vector3.one * 0.8f;

        var sequence = DOTween.Sequence();
        sequence.SetTarget(transform);

        // Появление
        var fadeTween = _canvasGroup.DOFade(1f, _fadeInDuration).SetTarget(_canvasGroup);
        var scaleTween = transform.DOScale(1f, _fadeInDuration)
            .SetEase(Ease.OutBack)
            .SetTarget(transform);

        sequence.Append(fadeTween);
        sequence.Join(scaleTween);

        // Ожидание
        sequence.AppendInterval(_displayDuration);

        // Исчезновение
        var fadeOutTween = _canvasGroup.DOFade(0, _fadeOutDuration).SetTarget(_canvasGroup);
        var scaleOutTween = transform.DOScale(0.8f, _fadeOutDuration).SetTarget(transform);

        sequence.Append(fadeOutTween);
        sequence.Join(scaleOutTween);

        AddTween(sequence);
    }

    /// <summary>
    /// Публичный метод для остановки всех анимаций
    /// </summary>
    public void StopAllAnimations()
    {
        KillAllTweens();
    }
}