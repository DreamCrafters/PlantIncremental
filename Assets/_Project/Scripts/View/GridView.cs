using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Визуальное представление игровой сетки
/// </summary>
public class GridView : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float _cellSize = 1f;
    [SerializeField] private float _cellSpacing = 0.1f;
    [SerializeField] private Vector2 _gridOffset = new Vector2(-2.5f, -2.5f);
    
    [Header("Prefabs")]
    [SerializeField] private CellView _cellPrefab;
    
    [Header("Visual Elements")]
    [SerializeField] private Transform _gridContainer;
    [SerializeField] private Transform _effectsContainer;
    [SerializeField] private SpriteRenderer _gridBackground;
    
    [Header("UI Elements")]
    [SerializeField] private Canvas _floatingUICanvas;
    [SerializeField] private RewardPopup _rewardPopupPrefab;
    [SerializeField] private FloatingMessage _floatingMessagePrefab;
    [SerializeField] private PlantPreview _plantPreviewPrefab;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem _plantEffectPrefab;
    [SerializeField] private ParticleSystem _harvestEffectPrefab;
    
    [Header("Colors")]
    [SerializeField] private Color _fertileColor = new Color(0.4f, 0.8f, 0.3f, 0.3f);
    [SerializeField] private Color _rockyColor = new Color(0.6f, 0.5f, 0.4f, 0.3f);
    [SerializeField] private Color _unsuitableColor = new Color(0.3f, 0.2f, 0.2f, 0.3f);
    
    // Кэшированные компоненты
    private readonly Dictionary<Vector2Int, CellView> _cells = new();
    private readonly Queue<RewardPopup> _rewardPopupPool = new();
    private readonly Queue<FloatingMessage> _messagePool = new();
    
    // Состояние
    private Vector2Int _gridSize;
    private PlantPreview _currentPlantPreview;
    
    private void Awake()
    {
        InitializePools();
    }
    
    /// <summary>
    /// Инициализирует пулы объектов для оптимизации
    /// </summary>
    private void InitializePools()
    {
        // Создаем начальный пул всплывающих сообщений
        for (int i = 0; i < 5; i++)
        {
            CreateRewardPopup();
            CreateFloatingMessage();
        }
    }
    
    /// <summary>
    /// Инициализирует визуальную сетку
    /// </summary>
    public void InitializeGrid(Vector2Int size)
    {
        _gridSize = size;
        
        CreateGridBackground();
        SetupCamera();
    }
    
    /// <summary>
    /// Создает визуальную клетку в указанной позиции
    /// </summary>
    public CellView CreateCellView(Vector2Int position)
    {
        if (_cells.ContainsKey(position))
        {
            Debug.LogWarning($"Cell at position {position} already exists");
            return _cells[position];
        }
        
        CellView cellView = Instantiate(_cellPrefab, _gridContainer);
        var worldPos = GridToWorldPosition(position);
        cellView.transform.position = worldPos;
        
        cellView.Initialize(position, _cellSize);
        _cells[position] = cellView;
        
        return cellView;
    }
    
    /// <summary>
    /// Преобразует координаты сетки в мировые координаты
    /// </summary>
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        float totalCellSize = _cellSize + _cellSpacing;
        return new Vector3(
            gridPos.x * totalCellSize + _gridOffset.x,
            gridPos.y * totalCellSize + _gridOffset.y,
            0
        );
    }
    
    /// <summary>
    /// Преобразует мировые координаты в координаты сетки
    /// </summary>
    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        float totalCellSize = _cellSize + _cellSpacing;
        int x = Mathf.RoundToInt((worldPos.x - _gridOffset.x) / totalCellSize);
        int y = Mathf.RoundToInt((worldPos.y - _gridOffset.y) / totalCellSize);
        return new Vector2Int(x, y);
    }
    
    /// <summary>
    /// Создает фоновую подложку для сетки
    /// </summary>
    private void CreateGridBackground()
    {
        _gridBackground.transform.localPosition = Vector3.zero;
        _gridBackground.sortingOrder = -10;
        
        // Создаем простой белый спрайт для фона
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        
        var sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1);
        _gridBackground.sprite = sprite;
        _gridBackground.color = new Color(0.9f, 0.85f, 0.75f, 0.3f);
        
        // Масштабируем под размер сетки
        float totalWidth = _gridSize.x * (_cellSize + _cellSpacing);
        float totalHeight = _gridSize.y * (_cellSize + _cellSpacing);
        _gridBackground.transform.localScale = new Vector3(totalWidth, totalHeight, 1);
    }
    
    /// <summary>
    /// Настраивает камеру для оптимального вида сетки
    /// </summary>
    private void SetupCamera()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null) return;
        
        // Центрируем камеру на сетке
        float totalWidth = _gridSize.x * (_cellSize + _cellSpacing);
        float totalHeight = _gridSize.y * (_cellSize + _cellSpacing);
        
        var centerX = _gridOffset.x + totalWidth / 2 - _cellSize / 2;
        var centerY = _gridOffset.y + totalHeight / 2 - _cellSize / 2;
        
        mainCamera.transform.position = new Vector3(centerX, centerY, -10);
        
        // Настраиваем размер ортографической камеры
        if (mainCamera.orthographic)
        {
            float requiredSize = Mathf.Max(totalWidth, totalHeight) * 0.6f;
            mainCamera.orthographicSize = requiredSize;
        }
    }
    
    /// <summary>
    /// Показывает сообщение пользователю
    /// </summary>
    public void ShowMessage(string message, MessageType type)
    {
        var floatingMessage = GetFloatingMessage();
        floatingMessage.Show(message, type, Camera.main.transform.position + Vector3.up * 2);
    }
    
    /// <summary>
    /// Показывает всплывающее окно с наградой
    /// </summary>
    public void ShowRewardPopup(Vector3 worldPosition, int coins, int petals)
    {
        var popup = GetRewardPopup();
        popup.Show(worldPosition, coins, petals);
    }
    
    /// <summary>
    /// Корутина для следования превью за курсором
    /// </summary>
    private System.Collections.IEnumerator FollowCursor()
    {
        while (_currentPlantPreview != null && _currentPlantPreview.gameObject.activeSelf)
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            
            // Привязываем к сетке
            var gridPos = WorldToGridPosition(mousePos);
            if (IsValidGridPosition(gridPos))
            {
                var snappedPos = GridToWorldPosition(gridPos);
                _currentPlantPreview.transform.position = snappedPos;
                
                // Подсвечиваем клетку под курсором
                HighlightCell(gridPos);
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Подсвечивает клетку
    /// </summary>
    private void HighlightCell(Vector2Int position)
    {
        if (_cells.TryGetValue(position, out var cell))
        {
            // Убираем предыдущую подсветку
            foreach (var c in _cells.Values)
            {
                if (c != cell)
                {
                    c.SetHighlight(false);
                }
            }
            
            cell.SetHighlight(true);
        }
    }
    
    /// <summary>
    /// Проверяет, является ли позиция валидной для сетки
    /// </summary>
    private bool IsValidGridPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < _gridSize.x &&
               position.y >= 0 && position.y < _gridSize.y;
    }
    
    /// <summary>
    /// Получает цвет для типа почвы
    /// </summary>
    public Color GetSoilColor(SoilType soilType)
    {
        return soilType switch
        {
            SoilType.Fertile => _fertileColor,
            SoilType.Rocky => _rockyColor,
            SoilType.Unsuitable => _unsuitableColor,
            _ => Color.white
        };
    }
    
    /// <summary>
    /// Воспроизводит эффект посадки растения
    /// </summary>
    public void PlayPlantEffect(Vector3 position)
    {
        if (_plantEffectPrefab != null)
        {
            var parent = (_effectsContainer != null && _effectsContainer.gameObject.scene.IsValid()) ? _effectsContainer : transform;
            var effect = Instantiate(_plantEffectPrefab, position, Quaternion.identity, parent);
            Destroy(effect.gameObject, 2f);
        }
    }
    
    /// <summary>
    /// Воспроизводит эффект сбора урожая
    /// </summary>
    public void PlayHarvestEffect(Vector3 position)
    {
        if (_harvestEffectPrefab != null)
        {
            var parent = (_effectsContainer != null && _effectsContainer.gameObject.scene.IsValid()) ? _effectsContainer : transform;
            var effect = Instantiate(_harvestEffectPrefab, position, Quaternion.identity, parent);
            Destroy(effect.gameObject, 2f);
        }
    }
    
    /// <summary>
    /// Получает всплывающее окно награды из пула
    /// </summary>
    private RewardPopup GetRewardPopup()
    {
        if (_rewardPopupPool.Count == 0)
        {
            CreateRewardPopup();
        }
        
        return _rewardPopupPool.Dequeue();
    }
    
    /// <summary>
    /// Создает новое всплывающее окно награды
    /// </summary>
    private void CreateRewardPopup()
    {
        return;

        var uiParent = (_floatingUICanvas != null && _floatingUICanvas.gameObject.scene.IsValid()) ? _floatingUICanvas.transform : transform;
        var popup = Instantiate(_rewardPopupPrefab, uiParent);
        popup.OnComplete += () => _rewardPopupPool.Enqueue(popup);
        popup.gameObject.SetActive(false);
        _rewardPopupPool.Enqueue(popup);
    }
    
    /// <summary>
    /// Получает всплывающее сообщение из пула
    /// </summary>
    private FloatingMessage GetFloatingMessage()
    {
        if (_messagePool.Count == 0)
        {
            CreateFloatingMessage();
        }
        
        return _messagePool.Dequeue();
    }
    
    /// <summary>
    /// Создает новое всплывающее сообщение
    /// </summary>
    private void CreateFloatingMessage()
    {
        return;

        var uiParent = (_floatingUICanvas != null && _floatingUICanvas.gameObject.scene.IsValid()) ? _floatingUICanvas.transform : transform;
        var message = Instantiate(_floatingMessagePrefab, uiParent);
        message.OnComplete += () => _messagePool.Enqueue(message);
        message.gameObject.SetActive(false);
        _messagePool.Enqueue(message);
    }
    
    /// <summary>
    /// Анимация появления сетки при старте
    /// </summary>
    public void AnimateGridAppearance()
    {
        foreach (var kvp in _cells)
        {
            var cell = kvp.Value;
            var position = kvp.Key;
            
            // Начальное состояние
            cell.transform.localScale = Vector3.zero;
            cell.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
            
            // Задержка основана на расстоянии от центра
            float delay = (position.x + position.y) * 0.05f;
            
            // Анимация появления
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(delay);
            sequence.Append(cell.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
            sequence.Join(cell.GetComponent<SpriteRenderer>().DOFade(1f, 0.3f));
        }
    }
    
    /// <summary>
    /// Очищает сетку
    /// </summary>
    public void ClearGrid()
    {
        foreach (var cell in _cells.Values)
        {
            if (cell != null)
            {
                Destroy(cell.gameObject);
            }
        }
        
        _cells.Clear();
    }
    
    private void OnDestroy()
    {
        ClearGrid();
        DOTween.Kill(transform);
    }
}