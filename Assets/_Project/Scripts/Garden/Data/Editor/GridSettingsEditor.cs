using UnityEditor;

[CustomEditor(typeof(GridSettings))]
public class GridSettingsEditor : Editor
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

        var gridSettings = (GridSettings)target;

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
                if (gridSettings.DisplayType == GridDisplayType.Isometric)
                {
                    EditorGUILayout.PropertyField(_isometricTileSizeProperty);
                }
                else if (gridSettings.DisplayType == GridDisplayType.Orthogonal)
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

        serializedObject.ApplyModifiedProperties();
    }
}