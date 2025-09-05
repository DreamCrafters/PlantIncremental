using UnityEngine;

[CreateAssetMenu(fileName = "GridSettings", menuName = "Game/Settings/Grid Settings")]
public class GridSettings : ScriptableObject
{
    [Header("Grid Configuration")]
    public Vector2Int GridSize = new(6, 6);
    public GridDisplayType DisplayType = GridDisplayType.Orthogonal;
    
    public Vector2 OrthographicTileSize = new(0.5f, 0.25f);
    public Vector2 IsometricTileSize = new(0.5f, 0.25f);
    
    [Header("Camera")]
    [Tooltip("Отступ от края карты до края камеры (в мировых единицах)")]
    public float CameraMargin = 1.0f;
}