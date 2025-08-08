using UnityEngine;
using UnityEditor;

public class RaritySystemTest : EditorWindow
{
    private GameSettings gameSettings;
    private int testIterations = 1000;
    private string testResults = "";
    private Vector2 scrollPosition = Vector2.zero;

    [MenuItem("Tools/Plant Rarity Test")]
    public static void ShowWindow()
    {
        GetWindow<RaritySystemTest>("Rarity System Test");
    }

    private void OnGUI()
    {
        GUILayout.Label("Plant Rarity System Test", EditorStyles.boldLabel);

        gameSettings = (GameSettings)EditorGUILayout.ObjectField("Game Settings", gameSettings, typeof(GameSettings), false);
        testIterations = EditorGUILayout.IntField("Test Iterations", testIterations);

        if (GUILayout.Button("Test Rarity Distribution"))
        {
            TestRarityDistribution();
        }

        if (GUILayout.Button("Show Normalized Chances"))
        {
            if (gameSettings != null)
            {
                var normalizedChances = gameSettings.GetNormalizedRarityChances();
                var results = "=== Normalized Rarity Chances ===\n";

                foreach (var chance in normalizedChances)
                {
                    results += $"{chance.Rarity}: {chance.Chance:P2}\n";
                }

                Debug.Log(results);
            }
        }

        if (!string.IsNullOrEmpty(testResults))
        {
            GUILayout.Label("Test Results:", EditorStyles.boldLabel);

            // Создаем область прокрутки с увеличенной высотой
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Используем стиль для лучшего отображения текста
            GUIStyle textStyle = new GUIStyle(EditorStyles.textArea);
            textStyle.wordWrap = true;
            textStyle.richText = false;

            EditorGUILayout.TextArea(testResults, textStyle, GUILayout.ExpandHeight(true));

            EditorGUILayout.EndScrollView();
        }
        
        GUILayout.Space(1);
    }

    private void TestRarityDistribution()
    {
        if (gameSettings == null)
        {
            testResults = "Please assign GameSettings first!";
            return;
        }

        var rarityCountMap = new System.Collections.Generic.Dictionary<PlantRarity, int>();
        
        // Инициализируем счётчики
        foreach (PlantRarity rarity in System.Enum.GetValues(typeof(PlantRarity)))
        {
            rarityCountMap[rarity] = 0;
        }

        // Симулируем выбор растений
        for (int i = 0; i < testIterations; i++)
        {
            var selectedRarity = SimulateGetRandomRarity();
            rarityCountMap[selectedRarity]++;
        }

        // Формируем результаты
        var results = new System.Text.StringBuilder();
        results.AppendLine($"=== Rarity Test Results ({testIterations} iterations) ===\n");
        
        results.AppendLine("Expected vs Actual distribution:");
        var normalizedChances = gameSettings.GetNormalizedRarityChances();
        foreach (var rarityChance in normalizedChances)
        {
            int actualCount = rarityCountMap[rarityChance.Rarity];
            float actualPercentage = (float)actualCount / testIterations;
            float expectedPercentage = rarityChance.Chance;
            float difference = Mathf.Abs(actualPercentage - expectedPercentage);
            
            results.AppendLine($"{rarityChance.Rarity}:");
            results.AppendLine($"  Expected: {expectedPercentage:P2}");
            results.AppendLine($"  Actual: {actualPercentage:P2} ({actualCount})");
            results.AppendLine($"  Difference: {difference:P2}");
            results.AppendLine();
        }

        testResults = results.ToString();
    }

    private PlantRarity SimulateGetRandomRarity()
    {
        var rarityChances = gameSettings.GetNormalizedRarityChances();
        if (rarityChances == null || rarityChances.Length == 0)
        {
            return PlantRarity.Common;
        }

        float randomValue = Random.Range(0f, 1f);
        float cumulativeChance = 0f;

        foreach (var rarityChance in rarityChances)
        {
            cumulativeChance += rarityChance.Chance;
            if (randomValue <= cumulativeChance)
            {
                return rarityChance.Rarity;
            }
        }

        return rarityChances[rarityChances.Length - 1].Rarity;
    }
}
