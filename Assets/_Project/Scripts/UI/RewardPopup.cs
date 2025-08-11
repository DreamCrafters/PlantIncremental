using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Всплывающее окно с наградой
/// </summary>
public class RewardPopup : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text _coinsText;
    [SerializeField] private TMP_Text _petalsText;
    [SerializeField] private Image _coinsIcon;
    [SerializeField] private Image _petalsIcon;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float _moveUpDistance = 50f;
    [SerializeField] private float _animationDuration = 1.5f;

    public event Action OnComplete;

    // Отслеживание твинов
    private readonly List<Tween> _activeTweens = new();

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
    /// Показывает всплывающее окно с наградой
    /// </summary>
    public void Show(Vector3 worldPosition, int coins, int petals)
    {
        // Преобразуем мировые координаты в экранные
        var screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        transform.position = screenPos;

        // Настраиваем текст
        _coinsText.text = $"+{coins}";
        _petalsText.text = $"+{petals}";

        // Скрываем элементы если награды нет
        _coinsText.gameObject.SetActive(coins > 0);
        _coinsIcon.gameObject.SetActive(coins > 0);
        _petalsText.gameObject.SetActive(petals > 0);
        _petalsIcon.gameObject.SetActive(petals > 0);

        gameObject.SetActive(true);
        AnimatePopup();
    }

    private void AnimatePopup()
    {
        if (transform == null || _canvasGroup == null) return;

        // Начальное состояние
        _canvasGroup.alpha = 0;
        transform.localScale = Vector3.zero;

        var sequence = DOTween.Sequence();
        sequence.SetTarget(transform);

        // Появление
        var scaleTween = transform.DOScale(1.2f, 0.2f)
            .SetEase(Ease.OutBack)
            .SetTarget(transform);
        var fadeTween = _canvasGroup.DOFade(1f, 0.2f)
            .SetTarget(_canvasGroup);

        sequence.Append(scaleTween);
        sequence.Join(fadeTween);

        // Небольшая пауза
        sequence.AppendInterval(0.5f);

        // Движение вверх и исчезновение
        var moveTween = transform.DOMoveY(transform.position.y + _moveUpDistance, _animationDuration)
            .SetEase(Ease.OutQuad)
            .SetTarget(transform);
        var fadeOutTween = _canvasGroup.DOFade(0, _animationDuration * 0.5f)
            .SetDelay(_animationDuration * 0.5f)
            .SetTarget(_canvasGroup);

        sequence.Append(moveTween);
        sequence.Join(fadeOutTween);

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