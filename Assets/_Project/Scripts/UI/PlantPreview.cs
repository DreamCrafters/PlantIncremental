using DG.Tweening;
using UnityEngine;

/// <summary>
/// Превью растения при выборе
/// </summary>
public class PlantPreview : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer _plantSprite;
    [SerializeField] private SpriteRenderer _validIndicator;
    [SerializeField] private SpriteRenderer _invalidIndicator;
    
    [Header("Settings")]
    [SerializeField] private float _alphaValid = 0.7f;
    [SerializeField] private float _alphaInvalid = 0.3f;
    
    private PlantData _currentPlant;
    
    private void Awake()
    {
        if (_plantSprite == null)
        {
            _plantSprite = GetComponent<SpriteRenderer>();
            if (_plantSprite == null)
            {
                _plantSprite = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        
        CreateIndicators();
    }
    
    /// <summary>
    /// Устанавливает растение для превью
    /// </summary>
    public void SetPlant(PlantData plantData)
    {
        _currentPlant = plantData;
        
        if (plantData != null && plantData.GrowthStages != null && plantData.GrowthStages.Length > 0)
        {
            _plantSprite.sprite = plantData.GrowthStages[0];
            _plantSprite.color = new Color(1, 1, 1, _alphaValid);
        }
    }
    
    /// <summary>
    /// Устанавливает валидность позиции для посадки
    /// </summary>
    public void SetValid(bool isValid)
    {
        _plantSprite.color = new Color(1, 1, 1, isValid ? _alphaValid : _alphaInvalid);
        
        if (_validIndicator != null)
            _validIndicator.enabled = isValid;
        
        if (_invalidIndicator != null)
            _invalidIndicator.enabled = !isValid;
        
        // Анимация
        if (isValid)
        {
            transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutBack);
        }
        else
        {
            transform.DOShakePosition(0.2f, 0.1f, 10);
        }
    }
    
    private void CreateIndicators()
    {
        // Индикатор валидной позиции
        if (_validIndicator == null)
        {
            var validGO = new GameObject("ValidIndicator");
            validGO.transform.SetParent(transform);
            validGO.transform.localPosition = Vector3.zero;
            _validIndicator = validGO.AddComponent<SpriteRenderer>();
            _validIndicator.sortingOrder = _plantSprite.sortingOrder - 1;
            _validIndicator.color = new Color(0.2f, 1f, 0.2f, 0.3f);
            _validIndicator.enabled = false;
            
            // Создаем круглый индикатор
            CreateCircleSprite(_validIndicator, Color.green);
        }
        
        // Индикатор невалидной позиции
        if (_invalidIndicator == null)
        {
            var invalidGO = new GameObject("InvalidIndicator");
            invalidGO.transform.SetParent(transform);
            invalidGO.transform.localPosition = Vector3.zero;
            _invalidIndicator = invalidGO.AddComponent<SpriteRenderer>();
            _invalidIndicator.sortingOrder = _plantSprite.sortingOrder - 1;
            _invalidIndicator.color = new Color(1f, 0.2f, 0.2f, 0.3f);
            _invalidIndicator.enabled = false;
            
            // Создаем X индикатор
            CreateXSprite(_invalidIndicator, Color.red);
        }
    }
    
    private void CreateCircleSprite(SpriteRenderer renderer, Color color)
    {
        var texture = new Texture2D(32, 32);
        var center = new Vector2(16, 16);
        
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist < 14 && dist > 12)
                {
                    texture.SetPixel(x, y, color);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        renderer.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 32);
    }
    
    private void CreateXSprite(SpriteRenderer renderer, Color color)
    {
        var texture = new Texture2D(32, 32);
        
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                // Рисуем X
                if (Mathf.Abs(x - y) < 2 || Mathf.Abs(x - (31 - y)) < 2)
                {
                    texture.SetPixel(x, y, color);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        renderer.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f, 32);
    }
}