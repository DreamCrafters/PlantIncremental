using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// UI панель выбора растения для посадки
/// </summary>
public class PlantSelectionPanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform _plantButtonsContainer;
    [SerializeField] private PlantSelectionButton _buttonPrefab;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Button _closeButton;
    
    [Header("Animation")]
    [SerializeField] private float _animationDuration = 0.3f;
    
    private readonly List<PlantSelectionButton> _buttons = new();
    private Action<PlantData> _onPlantSelected;
    private Vector2Int _targetPosition;
    
    private void Awake()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
        
        if (_closeButton != null)
            _closeButton.onClick.AddListener(Close);
    }
    
    /// <summary>
    /// Показывает панель выбора растений
    /// </summary>
    public void Show(PlantData[] availablePlants, Vector2Int gridPosition, Action<PlantData> onSelected)
    {
        _targetPosition = gridPosition;
        _onPlantSelected = onSelected;
        
        // Очищаем старые кнопки
        ClearButtons();
        
        // Создаем кнопки для каждого растения
        foreach (var plant in availablePlants)
        {
            CreatePlantButton(plant);
        }
        
        gameObject.SetActive(true);
        AnimateShow();
    }
    
    private void CreatePlantButton(PlantData plantData)
    {
        var button = Instantiate(_buttonPrefab, _plantButtonsContainer);
        button.Setup(plantData, () => OnPlantButtonClicked(plantData));
        _buttons.Add(button);
    }
    
    private void OnPlantButtonClicked(PlantData plantData)
    {
        _onPlantSelected?.Invoke(plantData);
        Close();
    }
    
    private void AnimateShow()
    {
        _canvasGroup.alpha = 0;
        transform.localScale = Vector3.one * 0.8f;
        
        var sequence = DOTween.Sequence();
        sequence.Append(_canvasGroup.DOFade(1f, _animationDuration));
        sequence.Join(transform.DOScale(1f, _animationDuration).SetEase(Ease.OutBack));
    }
    
    public void Close()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(_canvasGroup.DOFade(0, _animationDuration * 0.5f));
        sequence.Join(transform.DOScale(0.8f, _animationDuration * 0.5f));
        sequence.OnComplete(() => gameObject.SetActive(false));
    }
    
    private void ClearButtons()
    {
        foreach (var button in _buttons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        _buttons.Clear();
    }
    
    private void OnDestroy()
    {
        ClearButtons();
    }
}