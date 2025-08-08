using System;
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
    [SerializeField] private Color _infoColor = new Color(0.2f, 0.5f, 1f);
    [SerializeField] private Color _successColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color _warningColor = new Color(1f, 0.7f, 0.2f);
    [SerializeField] private Color _errorColor = new Color(1f, 0.2f, 0.2f);
    
    [Header("Animation")]
    [SerializeField] private float _displayDuration = 2f;
    [SerializeField] private float _fadeInDuration = 0.3f;
    [SerializeField] private float _fadeOutDuration = 0.5f;
    
    public event Action OnComplete;
    
    private void Awake()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
        
        if (_background == null)
            _background = GetComponent<Image>();
    }
    
    /// <summary>
    /// Показывает сообщение
    /// </summary>
    public void Show(string message, MessageType type, Vector3 worldPosition)
    {
        var screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        transform.position = screenPos;
        
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
        _canvasGroup.alpha = 0;
        transform.localScale = Vector3.one * 0.8f;
        
        var sequence = DOTween.Sequence();
        
        // Появление
        sequence.Append(_canvasGroup.DOFade(1f, _fadeInDuration));
        sequence.Join(transform.DOScale(1f, _fadeInDuration).SetEase(Ease.OutBack));
        
        // Ожидание
        sequence.AppendInterval(_displayDuration);
        
        // Исчезновение
        sequence.Append(_canvasGroup.DOFade(0, _fadeOutDuration));
        sequence.Join(transform.DOScale(0.8f, _fadeOutDuration));
        
        sequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
            OnComplete?.Invoke();
        });
    }
}