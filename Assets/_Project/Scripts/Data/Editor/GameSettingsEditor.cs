using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameSettings))]
public class GameSettingsEditor : Editor
{
    private SerializedProperty _gridSizeProperty;
    private SerializedProperty _displayTypeProperty;
    private SerializedProperty _isometricTileSizeProperty;
    private SerializedProperty _orthographicTileSizeProperty;
    private SerializedProperty _autoSaveIntervalProperty;
    private SerializedProperty _dayDurationProperty;
    private SerializedProperty _viewPrefabProperty;
    private SerializedProperty _availablePlantsProperty;
    private SerializedProperty _rarityChancesProperty;
    private SerializedProperty _cameraMarginProperty;

    private void OnEnable()
    {
        _gridSizeProperty = serializedObject.FindProperty("GridSize");
        _displayTypeProperty = serializedObject.FindProperty("DisplayType");
        _isometricTileSizeProperty = serializedObject.FindProperty("IsometricTileSize");
        _orthographicTileSizeProperty = serializedObject.FindProperty("OrthographicTileSize");
        _autoSaveIntervalProperty = serializedObject.FindProperty("AutoSaveInterval");
        _dayDurationProperty = serializedObject.FindProperty("DayDuration");
        _viewPrefabProperty = serializedObject.FindProperty("ViewPrefab");
        _availablePlantsProperty = serializedObject.FindProperty("AvailablePlants");
        _rarityChancesProperty = serializedObject.FindProperty("RarityChances");
        _cameraMarginProperty = serializedObject.FindProperty("CameraMargin");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var gameSettings = (GameSettings)target;

        EditorGUILayout.PropertyField(_gridSizeProperty);
        EditorGUILayout.PropertyField(_displayTypeProperty);

        if (gameSettings.DisplayType == GridDisplayType.Isometric)
        {
            EditorGUILayout.PropertyField(_isometricTileSizeProperty);
        }
        else if (gameSettings.DisplayType == GridDisplayType.Orthogonal)
        {
            EditorGUILayout.PropertyField(_orthographicTileSizeProperty);
        }

        EditorGUILayout.PropertyField(_cameraMarginProperty);
        EditorGUILayout.PropertyField(_autoSaveIntervalProperty);
        EditorGUILayout.PropertyField(_dayDurationProperty);
        EditorGUILayout.PropertyField(_viewPrefabProperty);
        EditorGUILayout.PropertyField(_availablePlantsProperty);
        EditorGUILayout.PropertyField(_rarityChancesProperty);
        
        // Показываем текущую сумму шансов
        float totalChance = 0f;
        if (gameSettings.RarityChances != null)
        {
            foreach (var chance in gameSettings.RarityChances)
            {
                totalChance += chance.Chance;
            }
        }

        // Кнопки управления
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Reset to Default"))
        {
            gameSettings.RarityChances = new PlantRarityChance[]
            {
                new() { Rarity = PlantRarity.Common, Chance = 0.6f },
                new() { Rarity = PlantRarity.Uncommon, Chance = 0.25f },
                new() { Rarity = PlantRarity.Rare, Chance = 0.1f },
                new() { Rarity = PlantRarity.Epic, Chance = 0.04f },
                new() { Rarity = PlantRarity.Legendary, Chance = 0.01f }
            };
            EditorUtility.SetDirty(gameSettings);
        }

        EditorGUILayout.EndHorizontal();

        // Статистика растений по редкости
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Show Plant Statistics"))
        {
            var plantCounts = gameSettings.GetPlantCountByRarity();
            var stats = "=== Plant Statistics by Rarity ===\n";
            
            foreach (var kvp in plantCounts)
            {
                stats += $"{kvp.Key}: {kvp.Value} plants\n";
            }
            
            if (plantCounts.Count == 0)
            {
                stats += "No plants assigned.\n";
            }
            
            Debug.Log(stats);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
