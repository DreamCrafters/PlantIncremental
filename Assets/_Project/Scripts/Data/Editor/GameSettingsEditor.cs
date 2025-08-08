using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameSettings))]
public class GameSettingsEditor : Editor
{
    private SerializedProperty gridSizeProperty;
    private SerializedProperty autoSaveIntervalProperty;
    private SerializedProperty dayDurationProperty;
    private SerializedProperty availablePlantsProperty;
    private SerializedProperty rarityChancesProperty;

    private void OnEnable()
    {
        gridSizeProperty = serializedObject.FindProperty("GridSize");
        autoSaveIntervalProperty = serializedObject.FindProperty("AutoSaveInterval");
        dayDurationProperty = serializedObject.FindProperty("DayDuration");
        availablePlantsProperty = serializedObject.FindProperty("AvailablePlants");
        rarityChancesProperty = serializedObject.FindProperty("RarityChances");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var gameSettings = (GameSettings)target;

        EditorGUILayout.PropertyField(gridSizeProperty);
        EditorGUILayout.PropertyField(autoSaveIntervalProperty);
        EditorGUILayout.PropertyField(dayDurationProperty);
        EditorGUILayout.PropertyField(availablePlantsProperty);
        EditorGUILayout.PropertyField(rarityChancesProperty);
        
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
