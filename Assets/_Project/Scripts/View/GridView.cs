using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using VContainer;

/// <summary>
/// Визуальное представление игровой сетки
/// </summary>
public class GridView : MonoBehaviour
{
    [Inject] private readonly GameSettings _gameSettings;

    [Header("Prefabs")]
    [SerializeField] private GridCellView _cellPrefab;
    [SerializeField] private RewardPopup _rewardPopupPrefab;
    [SerializeField] private FloatingMessage _floatingMessagePrefab;

    [Header("Visual Elements")]
    [SerializeField] private Transform _gridContainer;
    [SerializeField] private Canvas _floatingUICanvas;

    private readonly Dictionary<Vector2Int, GridCellView> _cells = new();
    private readonly Queue<RewardPopup> _rewardPopupPool = new();
    private readonly Queue<FloatingMessage> _messagePool = new();
    private readonly List<Tween> _activeTweens = new();

    private Vector2Int _gridSize;
    private Vector3 _boundsMin;
    private Vector3 _boundsMax;
    private Vector3 _boundsCenter;

    private void Awake()
    {
        InitializePools();
    }

    private void OnDestroy()
    {
        KillAllTweens();
        ClearGrid();
        DOTween.Kill(transform);
    }

    /// <summary>
    /// Безопасно убивает все активные твины
    /// </summary>
    private void KillAllTweens()
    {
        // Отменяем все активные твины
        for (int i = _activeTweens.Count - 1; i >= 0; i--)
        {
            if (_activeTweens[i] != null && _activeTweens[i].IsActive())
            {
                _activeTweens[i].Kill();
            }
        }
        _activeTweens.Clear();
    }

    /// <summary>
    /// Инициализирует визуальную сетку
    /// </summary>
    public void InitializeGrid(Vector2Int size)
    {
        _gridSize = size;
        if (_gridContainer == null) _gridContainer = transform;

        RecalculateBounds();
        _gridContainer.position = -_boundsCenter;
        SetupCamera();
    }

    /// <summary>
    /// Создает визуальную клетку в указанной позиции
    /// </summary>
    public GridCellView CreateCellView(Vector2Int position)
    {
        if (_cells.ContainsKey(position))
        {
            var existingCell = _cells[position];
            if (existingCell != null && existingCell.gameObject != null)
            {
                Debug.LogWarning($"Cell at position {position} already exists");
                return existingCell;
            }
            else
            {
                // Удаляем запись о null-объекте
                _cells.Remove(position);
            }
        }

        if (_cellPrefab == null)
        {
            Debug.LogError("Cell prefab is not assigned!");
            return null;
        }

        GridCellView cellView = Instantiate(_cellPrefab, _gridContainer);
        if (cellView == null)
        {
            Debug.LogError("Failed to instantiate cell view!");
            return null;
        }

        var localPos = GridToWorldPosition(position);

        float z = (_gameSettings != null && _gameSettings.DisplayType == GridDisplayType.Isometric)
            ? -(position.x + position.y) * 0.001f
            : -position.y * 0.001f;

        cellView.transform.localPosition = new Vector3(localPos.x, localPos.y, z);
        _cells[position] = cellView;

        return cellView;
    }

    /// <summary>
    /// Преобразует координаты сетки в мировые координаты
    /// </summary>
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return _gameSettings == null || _gameSettings.DisplayType == GridDisplayType.Orthogonal
            ? GridToWorldPositionOrthogonal(gridPos)
            : GridToWorldPositionIsometric(gridPos);
    }

    /// <summary>
    /// Показывает сообщение пользователю
    /// </summary>
    public void ShowMessage(string message, MessageType type)
    {
        var floatingMessage = GetFloatingMessage();
        if (floatingMessage == null) return;
        floatingMessage.Show(message, type);
    }

    /// <summary>
    /// Показывает всплывающее окно с наградой
    /// </summary>
    public void ShowRewardPopup(Vector3 worldPosition, int coins, int petals)
    {
        var popup = GetRewardPopup();
        if (popup == null) return;
        popup.Show(worldPosition, coins, petals);
    }

    /// <summary>
    /// Анимация появления сетки при старте
    /// </summary>
    public void AnimateGridAppearance()
    {
        // Очищаем предыдущие твины перед созданием новых
        KillAllTweens();

        foreach (var kvp in _cells)
        {
            var cell = kvp.Value;
            var position = kvp.Key;

            // Дополнительные проверки на валидность объекта
            if (cell == null || cell.gameObject == null) continue;

            // Убиваем все твины для данного объекта перед созданием новых
            DOTween.Kill(cell.transform);
            var spriteRenderer = cell.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                DOTween.Kill(spriteRenderer);
            }

            // Начальное состояние
            cell.transform.localScale = Vector3.zero;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1, 1, 1, 0);
            }

            // Задержка основана на расстоянии от центра
            float delay = (position.x + position.y) * 0.05f;

            // Анимация появления
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(delay);

            var scaleTween = cell.transform.DOScale(Vector3.one, 0.3f)
                .SetEase(Ease.OutBack)
                .SetTarget(cell.transform);
            sequence.Append(scaleTween);

            if (spriteRenderer != null)
            {
                var fadeTween = spriteRenderer.DOFade(1f, 0.3f)
                    .SetTarget(spriteRenderer);
                sequence.Join(fadeTween);
            }

            // Устанавливаем target для sequence и добавляем безопасные колбэки
            sequence.SetTarget(cell.transform);
            
            // Добавляем последовательность в список активных твинов
            _activeTweens.Add(sequence);

            // Безопасный колбэк с проверкой валидности
            sequence.OnComplete(() =>
            {
                if (sequence != null)
                {
                    _activeTweens.Remove(sequence);
                }
            });

            // Дополнительная безопасность - удаляем из списка при убийстве
            sequence.OnKill(() =>
            {
                if (sequence != null)
                {
                    _activeTweens.Remove(sequence);
                }
            });
        }
    }

    /// <summary>
    /// Очищает сетку
    /// </summary>
    public void ClearGrid()
    {
        // Сначала убиваем все твины
        KillAllTweens();

        // Отменяем все твины связанные с клетками перед их уничтожением
        foreach (var cell in _cells.Values)
        {
            if (cell != null && cell.gameObject != null)
            {
                // Убиваем все твины для данного объекта
                DOTween.Kill(cell.transform);
                DOTween.Kill(cell.gameObject);
                
                if (cell.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                {
                    DOTween.Kill(spriteRenderer);
                }
                
                // Уничтожаем объект
                if (Application.isPlaying)
                {
                    Destroy(cell.gameObject);
                }
                else
                {
                    DestroyImmediate(cell.gameObject);
                }
            }
        }

        _cells.Clear();
    }

    /// <summary>
    /// Инициализирует пулы объектов для оптимизации
    /// </summary>
    private void InitializePools()
    {
        for (int i = 0; i < 5; i++)
        {
            CreateRewardPopup();
            CreateFloatingMessage();
        }
    }

    // Вспомогательный метод для пересчета границ
    private Vector3 GridToWorldPositionForBounds(Vector2Int gridPos)
    {
        return _gameSettings == null || _gameSettings.DisplayType == GridDisplayType.Orthogonal
            ? GridToWorldPositionOrthogonal(gridPos)
            : GridToWorldPositionIsometric(gridPos);
    }

    /// <summary>
    /// Преобразует координаты сетки в мировые координаты для ортогонального отображения
    /// </summary>
    private Vector3 GridToWorldPositionOrthogonal(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * _gameSettings.OrthographicTileSize.x,
            gridPos.y * _gameSettings.OrthographicTileSize.y,
            0
        );
    }

    /// <summary>
    /// Преобразует координаты сетки в мировые координаты для изометрического отображения
    /// </summary>
    private Vector3 GridToWorldPositionIsometric(Vector2Int gridPos)
    {
        float xFactor = _gameSettings.IsometricTileSize.x;
        float yFactor = _gameSettings.IsometricTileSize.y;

        float isoX = (gridPos.x - gridPos.y) * xFactor;
        float isoY = (gridPos.x + gridPos.y) * yFactor;

        return new Vector3(isoX, isoY, 0);
    }

    /// <summary>
    /// Пересчитывает границы сетки (локальные координаты до смещения контейнера)
    /// </summary>
    private void RecalculateBounds()
    {
        if (_gridSize.x <= 0 || _gridSize.y <= 0)
        {
            _boundsMin = _boundsMax = _boundsCenter = Vector3.zero;
            return;
        }

        Vector3 topLeft = GridToWorldPositionForBounds(new Vector2Int(0, _gridSize.y - 1));
        Vector3 bottomRight = GridToWorldPositionForBounds(new Vector2Int(_gridSize.x - 1, 0));
        Vector3 topRight = GridToWorldPositionForBounds(new Vector2Int(_gridSize.x - 1, _gridSize.y - 1));
        Vector3 bottomLeft = GridToWorldPositionForBounds(new Vector2Int(0, 0));

        float minX = Mathf.Min(Mathf.Min(topLeft.x, topRight.x), Mathf.Min(bottomLeft.x, bottomRight.x));
        float maxX = Mathf.Max(Mathf.Max(topLeft.x, topRight.x), Mathf.Max(bottomLeft.x, bottomRight.x));
        float minY = Mathf.Min(Mathf.Min(topLeft.y, topRight.y), Mathf.Min(bottomLeft.y, bottomRight.y));
        float maxY = Mathf.Max(Mathf.Max(topLeft.y, topRight.y), Mathf.Max(bottomLeft.y, bottomRight.y));

        _boundsMin = new Vector3(minX, minY, 0f);
        _boundsMax = new Vector3(maxX, maxY, 0f);
        _boundsCenter = (_boundsMin + _boundsMax) * 0.5f;
    }

    /// <summary>
    /// Настраивает камеру для оптимального вида сетки
    /// </summary>
    private void SetupCamera()
    {
        var mainCamera = Camera.main;
        if (mainCamera == null) return;
        if (_gridSize.x <= 0 || _gridSize.y <= 0) return;

        // Пересчитываем границы на случай изменения настроек
        RecalculateBounds();

        // Контейнер уже центрирован в (0,0), ставим камеру в центр
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);

        if (mainCamera.orthographic)
        {
            float width = _boundsMax.x - _boundsMin.x;
            float height = _boundsMax.y - _boundsMin.y;

            // Отступ примерно на размер тайла для приятной рамки
            float tileX = (_gameSettings != null && _gameSettings.DisplayType == GridDisplayType.Isometric)
                ? _gameSettings.IsometricTileSize.x
                : _gameSettings.OrthographicTileSize.x;
            float tileY = (_gameSettings != null && _gameSettings.DisplayType == GridDisplayType.Isometric)
                ? _gameSettings.IsometricTileSize.y
                : _gameSettings.OrthographicTileSize.y;
            float margin = Mathf.Max(tileX, tileY) * 0.75f;

            float halfW = width * 0.5f + margin;
            float halfH = height * 0.5f + margin;

            float sizeByHeight = halfH;
            float sizeByWidth = halfW / Mathf.Max(mainCamera.aspect, 0.0001f);
            mainCamera.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);
        }
    }

    /// <summary>
    /// Получает всплывающее окно награды из пула
    /// </summary>
    private RewardPopup GetRewardPopup()
    {
        // Очищаем пул от уничтоженных объектов
        while (_rewardPopupPool.Count > 0)
        {
            var popup = _rewardPopupPool.Peek();
            if (popup == null || popup.gameObject == null)
            {
                _rewardPopupPool.Dequeue();
            }
            else
            {
                return _rewardPopupPool.Dequeue();
            }
        }

        CreateRewardPopup();
        if (_rewardPopupPool.Count == 0)
        {
            Debug.LogWarning("RewardPopup prefab is not assigned or failed to create");
            return null;
        }

        return _rewardPopupPool.Dequeue();
    }

    /// <summary>
    /// Создает новое всплывающее окно награды
    /// </summary>
    private void CreateRewardPopup()
    {
        if (_rewardPopupPrefab == null || _floatingUICanvas == null) return;

        var uiParent = _floatingUICanvas.transform;
        var popup = Instantiate(_rewardPopupPrefab, uiParent);
        
        if (popup != null)
        {
            popup.OnComplete += () => {
                if (popup != null && popup.gameObject != null)
                {
                    _rewardPopupPool.Enqueue(popup);
                }
            };
            popup.gameObject.SetActive(false);
            _rewardPopupPool.Enqueue(popup);
        }
    }

    /// <summary>
    /// Получает всплывающее сообщение из пула
    /// </summary>
    private FloatingMessage GetFloatingMessage()
    {
        // Очищаем пул от уничтоженных объектов
        while (_messagePool.Count > 0)
        {
            var message = _messagePool.Peek();
            if (message == null || message.gameObject == null)
            {
                _messagePool.Dequeue();
            }
            else
            {
                return _messagePool.Dequeue();
            }
        }

        CreateFloatingMessage();
        if (_messagePool.Count == 0)
        {
            Debug.LogWarning("FloatingMessage prefab is not assigned or failed to create");
            return null;
        }

        return _messagePool.Dequeue();
    }

    /// <summary>
    /// Создает новое всплывающее сообщение
    /// </summary>
    private void CreateFloatingMessage()
    {
        if (_floatingMessagePrefab == null || _floatingUICanvas == null) return;

        var uiParent = _floatingUICanvas.transform;
        var message = Instantiate(_floatingMessagePrefab, uiParent);
        
        if (message != null)
        {
            message.OnComplete += () => {
                if (message != null && message.gameObject != null)
                {
                    _messagePool.Enqueue(message);
                }
            };
            message.gameObject.SetActive(false);
            _messagePool.Enqueue(message);
        }
    }
}