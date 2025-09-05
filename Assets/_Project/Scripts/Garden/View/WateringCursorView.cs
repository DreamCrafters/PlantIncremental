using UniRx;
using UnityEngine;
using VContainer;

/// <summary>
/// Компонент для отображения иконки полива у курсора мыши
/// </summary>
public class WateringCursorView : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private ParticleSystem _wateringEffect;
    [SerializeField] private SpriteRenderer _cursorIcon;
    [SerializeField] private Vector3 _cursorOffset = Vector3.zero;
    [SerializeField] private bool _followWorldPosition = true;

    private IWateringVisualizationService _wateringVisualizationService;
    private IInputService _inputService;
    private Camera _camera;
    private CompositeDisposable _disposables = new();

    [Inject]
    public void Construct(IWateringVisualizationService wateringVisualizationService, IInputService inputService)
    {
        _wateringVisualizationService = wateringVisualizationService;
        _inputService = inputService;
        _camera = Camera.main;
    }

    private void Start()
    {
        if (_cursorIcon == null)
        {
            Debug.LogError("WateringCursorView: Cursor icon is not assigned!");
            return;
        }

        // Изначально скрываем иконку
        _cursorIcon.enabled = false;

        // Подписываемся на состояние визуализации полива
        _wateringVisualizationService.IsWateringVisualizationActive
            .Subscribe(isActive => SetActivateCursorIcon(isActive))
            .AddTo(_disposables);

        // Обновляем позицию иконки
        if (_followWorldPosition)
        {
            // Следуем за мировой позицией курсора полива
            _wateringVisualizationService.WateringCursorWorldPosition
                .Where(_ => _wateringVisualizationService.IsWateringVisualizationActive.Value)
                .Subscribe(worldPos => UpdateIconPosition(worldPos))
                .AddTo(_disposables);
        }
        else
        {
            // Следуем за экранной позицией мыши
            _inputService.ScreenPositionLate
                .Where(_ => _wateringVisualizationService.IsWateringVisualizationActive.Value)
                .Subscribe(screenPos => UpdateIconScreenPosition(screenPos))
                .AddTo(_disposables);
        }
    }

    private void OnDestroy()
    {
        _disposables?.Dispose();
    }

    public void SetCursorOffset(Vector3 offset)
    {
        _cursorOffset = offset;
    }

    public void SetFollowWorldPosition(bool followWorld)
    {
        _followWorldPosition = followWorld;
    }

    private void SetActivateCursorIcon(bool active)
    {
        if (_cursorIcon != null)
        {
            _cursorIcon.enabled = active;

            if (active)
            {
                if (_followWorldPosition)
                {
                    UpdateIconPosition(_wateringVisualizationService.WateringCursorWorldPosition.Value);
                }
                else
                {
                    UpdateIconScreenPosition(_inputService.ScreenPositionLate.Value);
                }
            }
        }

        if (_wateringEffect != null)
        {
            if (active)
                _wateringEffect.Play();
            else
                _wateringEffect.Stop();
        }
    }

    private void UpdateIconPosition(Vector3 worldPosition)
    {
        if (_cursorIcon != null)
        {
            transform.position = worldPosition + _cursorOffset;
        }
    }

    private void UpdateIconScreenPosition(Vector2 screenPosition)
    {
        if (_cursorIcon != null && _camera != null)
        {
            transform.position = (Vector3)screenPosition + _cursorOffset;
        }
    }
}
