using System;
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
    [SerializeField] private AnimationCurve _fadeCurve;
    
    public event Action OnComplete;
    
    private void Awake()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
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
        // Начальное состояние
        _canvasGroup.alpha = 0;
        transform.localScale = Vector3.zero;
        
        var sequence = DOTween.Sequence();
        
        // Появление
        sequence.Append(transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
        sequence.Join(_canvasGroup.DOFade(1f, 0.2f));
        
        // Небольшая пауза
        sequence.AppendInterval(0.5f);
        
        // Движение вверх и исчезновение
        sequence.Append(transform.DOMoveY(transform.position.y + _moveUpDistance, _animationDuration)
            .SetEase(Ease.OutQuad));
        sequence.Join(_canvasGroup.DOFade(0, _animationDuration * 0.5f)
            .SetDelay(_animationDuration * 0.5f));
        
        sequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
            OnComplete?.Invoke();
        });
    }
}