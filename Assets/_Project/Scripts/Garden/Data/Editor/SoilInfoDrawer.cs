using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SoilInfo))]
public class SoilInfoDrawer : PropertyDrawer
{
    private const float ChanceTextWidth = 55f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var typeProperty = property.FindPropertyRelative("Type");
        var growingSpeedProperty = property.FindPropertyRelative("GrowingSpeed");
        var chanceProperty = property.FindPropertyRelative("Chance");

        // Получаем нормализованное значение
        float rawValue = chanceProperty.floatValue;
        float normalizedValue = GetNormalizedValue(property, rawValue);

        // Высота одной строки
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // Создаем прямоугольники для каждой строки
        var typeRect = new Rect(position.x, position.y, position.width, lineHeight);
        var growingSpeedRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);
        var chanceRect = new Rect(position.x, position.y + (lineHeight + spacing) * 2, position.width - 60f, lineHeight);
        var normalizedPercentRect = new Rect(position.x + position.width - ChanceTextWidth, position.y + (lineHeight + spacing) * 2, ChanceTextWidth, lineHeight);

        // Отображаем поля
        EditorGUI.PropertyField(typeRect, typeProperty, new GUIContent("Тип почвы"));
        EditorGUI.PropertyField(growingSpeedRect, growingSpeedProperty, new GUIContent("Скорость роста"));

        // Для Chance показываем как слайдер с процентами
        float chanceValue = chanceProperty.floatValue;
        chanceValue = EditorGUI.Slider(chanceRect, new GUIContent("Шанс"), chanceValue, 0f, 1f);
        chanceProperty.floatValue = chanceValue;

        // Нормализованные проценты выделяем цветом
        var originalColor = GUI.color;
        GUI.color = Color.green;
        EditorGUI.LabelField(normalizedPercentRect, $"→{normalizedValue:P1}", EditorStyles.miniLabel);
        GUI.color = originalColor;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Возвращаем высоту для трех строк с отступами
        return EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2;
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
