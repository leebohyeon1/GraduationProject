using UnityEditor;
using UnityEngine;

/// <summary>
/// EnemyUseAnything 배열을 위한 커스텀 프로퍼티 드로어
/// 디자이너가 스크립터블 오브젝트 배열을 더 쉽게 관리할 수 있도록 UI를 개선합니다.
/// </summary>
[CustomPropertyDrawer(typeof(EnemyUseAnything[]))]
public class EnemyUseAnythingArrayDrawer : PropertyDrawer
{
    private const float INDENT_WIDTH = 12f;
    private const float SPACING = 4f;
    private const float ELEMENT_HEIGHT = 20f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ArraySize)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        float x = position.x;
        float y = position.y;
        float width = position.width;

        int arraySize = property.arraySize;

        // 배열 크기 표시 및 버튼
        Rect sizeRect = new Rect(x, y, width, ELEMENT_HEIGHT);
        GUIContent sizeLabel = new GUIContent($"공격 효과 개수: {arraySize}", "배열의 크기");

        if (GUI.Button(sizeRect, sizeLabel, EditorStyles.toolbarButton))
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("요소 추가"), false, () => AddElement(property));
            menu.AddItem(new GUIContent("요소 제거"), false, () => RemoveElement(property));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("모두 제거"), false, () => ClearArray(property));
            menu.ShowAsContext();
        }

        y += ELEMENT_HEIGHT + SPACING;

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // 각 요소 표시
        for (int i = 0; i < arraySize; i++)
        {
            SerializedProperty element = property.GetArrayElementAtIndex(i);

            Rect elementRect = new Rect(x + INDENT_WIDTH, y, width - INDENT_WIDTH, ELEMENT_HEIGHT);

            if (element.objectReferenceValue != null)
            {
                string elementName = element.objectReferenceValue.name;
                string elementTypeName = element.objectReferenceValue.GetType().Name;

                GUIContent elementLabel = new GUIContent($"[{i}] {elementName} ({elementTypeName})",
                    "EnemyUseAnything 타입의 스크립터블 오브젝트");

                EditorGUI.PropertyField(elementRect, element, elementLabel);
            }
            else
            {
                GUIContent emptyLabel = new GUIContent($"[{i}] None (Empty)",
                    "여기에 EnemyUseAnything 타입의 스크립터블 오브젝트를 드래그 앤 드롭하세요.");

                EditorGUI.PropertyField(elementRect, element, emptyLabel);
            }

            // 마우스 우클릭 메뉴 (요소 제거)
            if (Event.current.type == EventType.ContextClick && elementRect.Contains(Event.current.mousePosition))
            {
                GenericMenu elementMenu = new GenericMenu();
                elementMenu.AddItem(new GUIContent("제거"), false, () => RemoveElementAt(property, i));
                elementMenu.AddItem(new GUIContent("중복 생성"), false, () => DuplicateElement(property, i));
                elementMenu.ShowAsContext();
                Event.current.Use();
            }

            y += ELEMENT_HEIGHT + SPACING;
        }

        // 빈 슬롯 표시 (드래그 앤 드롭용)
        if (arraySize == 0)
        {
            Rect emptyRect = new Rect(x + INDENT_WIDTH, y, width - INDENT_WIDTH, ELEMENT_HEIGHT * 2);
            EditorGUI.HelpBox(emptyRect, "공격 효과를 추가하려면 위 버튼을 클릭하거나 여기에 스크립터블 오브젝트를 드래그하세요.", MessageType.None);
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ArraySize)
            return EditorGUI.GetPropertyHeight(property, label);

        int arraySize = property.arraySize;
        float height = ELEMENT_HEIGHT + SPACING; // 크기 버튼

        if (arraySize == 0)
        {
            height += ELEMENT_HEIGHT * 2 + SPACING; // 빈 상태 메시지
        }
        else
        {
            height += arraySize * (ELEMENT_HEIGHT + SPACING); // 요소들
        }

        return height;
    }

    private void AddElement(SerializedProperty property)
    {
        property.arraySize++;
        property.serializedObject.ApplyModifiedProperties();
    }

    private void RemoveElement(SerializedProperty property)
    {
        if (property.arraySize > 0)
        {
            property.arraySize--;
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private void RemoveElementAt(SerializedProperty property, int index)
    {
        if (index >= 0 && index < property.arraySize)
        {
            property.DeleteArrayElementAtIndex(index);
            property.serializedObject.ApplyModifiedProperties();
        }
    }

    private void DuplicateElement(SerializedProperty property, int index)
    {
        if (index >= 0 && index < property.arraySize)
        {
            SerializedProperty element = property.GetArrayElementAtIndex(index);
            if (element.objectReferenceValue != null)
            {
                property.arraySize++;
                SerializedProperty newElement = property.GetArrayElementAtIndex(property.arraySize - 1);
                newElement.objectReferenceValue = element.objectReferenceValue;
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }

    private void ClearArray(SerializedProperty property)
    {
        property.arraySize = 0;
        property.serializedObject.ApplyModifiedProperties();
    }
}
