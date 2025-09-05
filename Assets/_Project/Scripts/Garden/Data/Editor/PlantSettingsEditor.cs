using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlantSettings))]
public class PlantSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var plantSettings = (PlantSettings)target;

        // Draw all properties with default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        
        // Показываем текущую сумму шансов для редкости растений
        float totalRarityChance = 0f;
        if (plantSettings.RarityChances != null)
        {
            foreach (var chance in plantSettings.RarityChances)
            {
                totalRarityChance += chance.Chance;
            }
        }

        EditorGUILayout.LabelField($"Total Rarity Chances: {totalRarityChance:F2}", EditorStyles.boldLabel);

        // Кнопка управления
        if (GUILayout.Button("Reset Rarity to Default"))
        {
            plantSettings.RarityChances = new PlantRarityChance[]
            {
                new() { Rarity = PlantRarity.Common, Chance = 0.6f },
                new() { Rarity = PlantRarity.Uncommon, Chance = 0.25f },
                new() { Rarity = PlantRarity.Rare, Chance = 0.1f },
                new() { Rarity = PlantRarity.Epic, Chance = 0.04f },
                new() { Rarity = PlantRarity.Legendary, Chance = 0.01f }
            };
            EditorUtility.SetDirty(plantSettings);
        }

        serializedObject.ApplyModifiedProperties();
    }
}