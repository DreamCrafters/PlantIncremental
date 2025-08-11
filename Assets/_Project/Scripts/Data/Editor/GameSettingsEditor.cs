using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameSettings))]
public class GameSettingsEditor : Editor
{
    private SerializedProperty _isometricTileSizeProperty;
    private SerializedProperty _orthographicTileSizeProperty;

    private void OnEnable()
    {
        _isometricTileSizeProperty = serializedObject.FindProperty("IsometricTileSize");
        _orthographicTileSizeProperty = serializedObject.FindProperty("OrthographicTileSize");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var gameSettings = (GameSettings)target;

        // Получаем итератор по всем видимым свойствам
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            
            // Пропускаем скрипт
            if (iterator.propertyPath == "m_Script")
                continue;

            // Специальная обработка для DisplayType
            if (iterator.name == "DisplayType")
            {
                EditorGUILayout.PropertyField(iterator);
                
                // Сразу после DisplayType отрисовываем соответствующий размер тайла
                if (gameSettings.DisplayType == GridDisplayType.Isometric)
                {
                    EditorGUILayout.PropertyField(_isometricTileSizeProperty);
                }
                else if (gameSettings.DisplayType == GridDisplayType.Orthogonal)
                {
                    EditorGUILayout.PropertyField(_orthographicTileSizeProperty);
                }
                continue;
            }
            
            // Пропускаем размеры тайлов, так как они уже отрисованы выше
            if (iterator.name == "IsometricTileSize" || iterator.name == "OrthographicTileSize")
                continue;
            
            // Для всех остальных полей - стандартная отрисовка
            EditorGUILayout.PropertyField(iterator, true);
        }
        
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
