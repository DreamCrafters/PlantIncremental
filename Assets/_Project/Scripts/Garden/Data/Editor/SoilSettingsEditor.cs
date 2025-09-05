using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SoilSettings))]
public class SoilSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var soilSettings = (SoilSettings)target;

        // Draw all properties with default inspector
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        
        // Показываем текущую сумму шансов для типов почвы
        float totalSoilChance = 0f;
        if (soilSettings.SoilInfo != null)
        {
            foreach (var chance in soilSettings.SoilInfo)
            {
                totalSoilChance += chance.Chance;
            }
        }

        EditorGUILayout.LabelField($"Total Soil Type Chances: {totalSoilChance:F2}", EditorStyles.boldLabel);

        // Кнопка управления
        if (GUILayout.Button("Reset Soil to Default"))
        {
            soilSettings.SoilInfo = new SoilInfo[]
            {
                new() { Type = SoilType.Fertile, Chance = 0.6f },
                new() { Type = SoilType.Rocky, Chance = 0.3f },
                new() { Type = SoilType.Unsuitable, Chance = 0.1f }
            };
            EditorUtility.SetDirty(soilSettings);
        }

        serializedObject.ApplyModifiedProperties();
    }
}