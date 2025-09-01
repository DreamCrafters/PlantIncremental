using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(SoilTypeChance))]
public class SoilTypeChanceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var typeProperty = property.FindPropertyRelative("Type");
        var chanceProperty = property.FindPropertyRelative("Chance");

        // Получаем нормализованное значение
        float rawValue = chanceProperty.floatValue;
        float normalizedValue = GetNormalizedValue(property, rawValue);

        // Разделяем rect на три части: Type, Slider, Normalized%
        var normalizedWidth = 50f;
        var typeWidth = (position.width - normalizedWidth) * 0.5f;
        var sliderWidth = (position.width - normalizedWidth) * 0.5f;

        var typeRect = new Rect(position.x, position.y, typeWidth - 2, position.height);
        var chanceRect = new Rect(position.x + typeWidth, position.y, sliderWidth - 2, position.height);
        var normalizedPercentRect = new Rect(position.x + typeWidth + sliderWidth, position.y, normalizedWidth, position.height);

        // Отображаем поля
        EditorGUI.PropertyField(typeRect, typeProperty, GUIContent.none);
        
        // Для Chance показываем как слайдер с процентами
        float chanceValue = chanceProperty.floatValue;
        chanceValue = EditorGUI.Slider(chanceRect, chanceValue, 0f, 1f);
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
        return EditorGUIUtility.singleLineHeight;
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
