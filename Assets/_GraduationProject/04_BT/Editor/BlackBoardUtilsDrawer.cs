using UnityEditor;
using UnityEngine;
[CustomPropertyDrawer(typeof(BlackBoardUtils))]
public class BlackBoardUtilsDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        // 전체 라벨 표시 (예: "Black Board Utils")
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        // 들여쓰기 설정
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;
        float yOffset = 0;
        // InputType
        SerializedProperty inputTypeProp = property.FindPropertyRelative("inputType");
        Rect inputTypeRect = new Rect(position.x, position.y + yOffset, position.width, lineHeight);
        EditorGUI.LabelField(inputTypeRect, new GUIContent("Input Type"));
        inputTypeRect.x += 80; // 라벨 너비
        inputTypeRect.width -= 80;
        EditorGUI.PropertyField(inputTypeRect, inputTypeProp, GUIContent.none);
        yOffset += lineHeight + spacing;
        // 키 필드 (InputType에 따라 다름)
        SerializedProperty enumKeyProp = property.FindPropertyRelative("enumKey");
        SerializedProperty stringKeyProp = property.FindPropertyRelative("stringKey");
        Rect keyRect = new Rect(position.x, position.y + yOffset, position.width, lineHeight);
        if (inputTypeProp.enumValueIndex == 0) // String
        {
            EditorGUI.LabelField(keyRect, new GUIContent("String Key"));
            keyRect.x += 80;
            keyRect.width -= 80;
            EditorGUI.PropertyField(keyRect, stringKeyProp, GUIContent.none);
        }
        else // Enum
        {
            EditorGUI.LabelField(keyRect, new GUIContent("Enum Key"));
            keyRect.x += 80;
            keyRect.width -= 80;
            EditorGUI.PropertyField(keyRect, enumKeyProp, GUIContent.none);
        }
        yOffset += lineHeight + spacing;
        // ValueType
        SerializedProperty valueTypeProp = property.FindPropertyRelative("valueType");
        Rect valueTypeRect = new Rect(position.x, position.y + yOffset, position.width, lineHeight);
        EditorGUI.LabelField(valueTypeRect, new GUIContent("Value Type"));
        valueTypeRect.x += 80;
        valueTypeRect.width -= 80;
        EditorGUI.PropertyField(valueTypeRect, valueTypeProp, GUIContent.none);
        yOffset += lineHeight + spacing;
        // 값 필드 (ValueType에 따라 다름)
        SerializedProperty intValueProp = property.FindPropertyRelative("intValue");
        SerializedProperty floatValueProp = property.FindPropertyRelative("floatValue");
        SerializedProperty boolValueProp = property.FindPropertyRelative("boolValue");
        SerializedProperty vector3ValueProp = property.FindPropertyRelative("vector3Value");
        SerializedProperty stringValueProp = property.FindPropertyRelative("stringValue");
        SerializedProperty transformObjectValueProp = property.FindPropertyRelative("transformObjectValue");
        Rect valueRect = new Rect(position.x, position.y + yOffset, position.width, lineHeight);
        
        switch (valueTypeProp.enumValueIndex)
        {
            case 0: // Integer
                EditorGUI.LabelField(valueRect, new GUIContent("Int Value"));
                valueRect.x += 80;
                valueRect.width -= 80;
                EditorGUI.PropertyField(valueRect, intValueProp, GUIContent.none);
                break;
            case 1: // Float
                EditorGUI.LabelField(valueRect, new GUIContent("Float Value"));
                valueRect.x += 80;
                valueRect.width -= 80;
                EditorGUI.PropertyField(valueRect, floatValueProp, GUIContent.none);
                break;
            case 2: // Boolean
                EditorGUI.LabelField(valueRect, new GUIContent("Bool Value"));
                valueRect.x += 80;
                valueRect.width -= 80;
                EditorGUI.PropertyField(valueRect, boolValueProp, GUIContent.none);
                break;
            case 3: // Vector3
                EditorGUI.LabelField(valueRect, new GUIContent("Vector3 Value"));
                valueRect.x += 80;
                valueRect.width -= 80;
                EditorGUI.PropertyField(valueRect, vector3ValueProp, GUIContent.none);
                break;
            case 4: // String
                EditorGUI.LabelField(valueRect, new GUIContent("String Value"));
                valueRect.x += 80;
                valueRect.width -= 80;
                EditorGUI.PropertyField(valueRect, stringValueProp, GUIContent.none);
                break;
            case 5: // Transform
                EditorGUI.LabelField(valueRect, new GUIContent("Transform"));
                valueRect.x += 80;
                valueRect.width -= 80;
                EditorGUI.PropertyField(valueRect, transformObjectValueProp, GUIContent.none);
                break;
        }
        // 들여쓰기 복원
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + 2) * 4;
    }
}
