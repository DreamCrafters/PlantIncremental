using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PlantRarityChance))]
public class PlantRarityChanceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var rarityProperty = property.FindPropertyRelative("Rarity");
        var chanceProperty = property.FindPropertyRelative("Chance");

        // Получаем нормализованное значение
        float rawValue = chanceProperty.floatValue;
        float normalizedValue = GetNormalizedValue(property, rawValue);

        // Высота одной строки
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // Создаем прямоугольники для каждой строки
        var rarityRect = new Rect(position.x, position.y, position.width, lineHeight);
        var chanceRect = new Rect(position.x, position.y + lineHeight + spacing, position.width - 60f, lineHeight);
        var normalizedPercentRect = new Rect(position.x + position.width - 55f, position.y + lineHeight + spacing, 55f, lineHeight);

        // Отображаем поля
        EditorGUI.PropertyField(rarityRect, rarityProperty, new GUIContent("Редкость"));
        
        // Для Chance показываем как слайдер с процентами
        float chanceValue = chanceProperty.floatValue;
        chanceValue = EditorGUI.Slider(chanceRect, new GUIContent("Шанс"), chanceValue, 0f, 1f);
        chanceProperty.floatValue = chanceValue;

        // Нормализованные проценты выделяем цветом
        var originalColor = GUI.color;
        GUI.color = Color.cyan;
        EditorGUI.LabelField(normalizedPercentRect, $"→{normalizedValue:P1}", EditorStyles.miniLabel);
        GUI.color = originalColor;
    
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Возвращаем высоту для двух строк с отступом
        return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
    }

    private float GetNormalizedValue(SerializedProperty currentProperty, float rawValue)
    {
        // Находим родительский массив
        var parentPath = currentProperty.propertyPath;
        var lastDot = parentPath.LastIndexOf('.');
        if (lastDot == -1) return rawValue;
        
        var arrayPath = parentPath.Substring(0, lastDot);
        var arrayProperty = currentProperty.serializedObject.FindProperty(arrayPath);
        
        if (arrayProperty == null || !arrayProperty.isArray) return rawValue;

        // Подсчитываем общую сумму
        float totalChance = 0f;
        for (int i = 0; i < arrayProperty.arraySize; i++)
        {
            var element = arrayProperty.GetArrayElementAtIndex(i);
            var chanceProperty = element.FindPropertyRelative("Chance");
            if (chanceProperty != null)
            {
                totalChance += chanceProperty.floatValue;
            }
        }

        // Возвращаем нормализованное значение
        return totalChance > 0f ? rawValue / totalChance : 0f;
    }
}
