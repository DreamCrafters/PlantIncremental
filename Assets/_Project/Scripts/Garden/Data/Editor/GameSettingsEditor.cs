using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameSettings))]
public class GameSettingsEditor : Editor
{
    private const string DisplayTypeFieldName = "DisplayType";
    private const string IsometricTileSizeFieldName = "IsometricTileSize";
    private const string OrthographicTileSizeFieldName = "OrthographicTileSize";

    private SerializedProperty _isometricTileSizeProperty;
    private SerializedProperty _orthographicTileSizeProperty;

    private void OnEnable()
    {
        _isometricTileSizeProperty = serializedObject.FindProperty(IsometricTileSizeFieldName);
        _orthographicTileSizeProperty = serializedObject.FindProperty(OrthographicTileSizeFieldName);
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
            if (iterator.name == DisplayTypeFieldName)
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
            if (iterator.name == IsometricTileSizeFieldName || iterator.name == OrthographicTileSizeFieldName)
                continue;
            
            // Для всех остальных полей - стандартная отрисовка
            EditorGUILayout.PropertyField(iterator, true);
        }
        
        // Показываем текущую сумму шансов для редкости растений
        float totalRarityChance = 0f;
        if (gameSettings.RarityChances != null)
        {
            foreach (var chance in gameSettings.RarityChances)
            {
                totalRarityChance += chance.Chance;
            }
        }

        EditorGUILayout.LabelField($"Total Rarity Chances: {totalRarityChance:F2}", EditorStyles.boldLabel);

        // Показываем текущую сумму шансов для типов почвы
        float totalSoilChance = 0f;
        if (gameSettings.SoilInfo != null)
        {
            foreach (var chance in gameSettings.SoilInfo)
            {
                totalSoilChance += chance.Chance;
            }
        }

        EditorGUILayout.LabelField($"Total Soil Type Chances: {totalSoilChance:F2}", EditorStyles.boldLabel);

        // Кнопки управления
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Reset Rarity to Default"))
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

        if (GUILayout.Button("Reset Soil to Default"))
        {
            gameSettings.SoilInfo = new SoilInfo[]
            {
                new() { Type = SoilType.Fertile, Chance = 0.6f },
                new() { Type = SoilType.Rocky, Chance = 0.3f },
                new() { Type = SoilType.Unsuitable, Chance = 0.1f }
            };
            EditorUtility.SetDirty(gameSettings);
        }

        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}
