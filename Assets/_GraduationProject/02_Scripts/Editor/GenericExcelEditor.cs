#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection; // 리플렉션 사용을 위해 추가

public class GenericExcelEditor : EditorWindow
{
    private ScriptableObject targetSO;
    private SerializedObject serializedObject;
    private SerializedProperty targetListProperty;

    private Vector2 scrollPos;
    private string searchQuery = "";

    private List<float> columnWidths = new List<float>();
    private bool isResizing = false;
    private int resizingColumnIndex = -1;

    [MenuItem("Tools/Universal Data Editor")]
    public static void ShowWindow()
    {
        GetWindow<GenericExcelEditor>("범용 데이터 에디터");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUI.BeginChangeCheck();
        targetSO = (ScriptableObject)EditorGUILayout.ObjectField("Target Data (SO)", targetSO, typeof(ScriptableObject), false);

        if (EditorGUI.EndChangeCheck() && targetSO != null)
        {
            serializedObject = new SerializedObject(targetSO);
            FindListProperty();
            columnWidths.Clear();
        }

        if (targetSO == null || targetListProperty == null)
        {
            EditorGUILayout.HelpBox(targetSO == null ? "데이터가 담긴 ScriptableObject를 여기에 드래그 앤 드롭 하세요." : "이 ScriptableObject에는 표시할 리스트나 배열이 없습니다.", MessageType.Info);
            return;
        }

        serializedObject.Update();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label("🔍 Search:", GUILayout.Width(70));
        searchQuery = EditorGUILayout.TextField(searchQuery);
        if (GUILayout.Button("Clear", GUILayout.Width(50)))
        {
            searchQuery = "";
            GUI.FocusControl(null);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        scrollPos = GUILayout.BeginScrollView(scrollPos);

        DrawHeader();

        for (int i = 0; i < targetListProperty.arraySize; i++)
        {
            SerializedProperty rowProp = targetListProperty.GetArrayElementAtIndex(i);

            if (!RowMatchesSearch(rowProp, searchQuery)) continue;

            GUILayout.BeginHorizontal("box");
            GUILayout.Label(i.ToString(), EditorStyles.boldLabel, GUILayout.Width(30));

            int colIndex = 0;

            if (rowProp.propertyType == SerializedPropertyType.ObjectReference)
            {
                EditorGUILayout.PropertyField(rowProp, GUIContent.none, GUILayout.Width(GetColumnWidth(colIndex++)));

                if (rowProp.objectReferenceValue != null)
                {
                    SerializedObject innerSO = new SerializedObject(rowProp.objectReferenceValue);
                    innerSO.Update();
                    SerializedProperty innerProp = innerSO.GetIterator();
                    bool enterChildren = true;

                    while (innerProp.NextVisible(enterChildren))
                    {
                        enterChildren = false;
                        if (innerProp.name == "m_Script") continue;

                        EditorGUILayout.PropertyField(innerProp, GUIContent.none, GUILayout.Width(GetColumnWidth(colIndex++)));
                    }
                    innerSO.ApplyModifiedProperties();
                }
            }
            else
            {
                SerializedProperty fieldProp = rowProp.Copy();
                SerializedProperty endProp = rowProp.GetEndProperty();
                bool isFirst = true;

                while (fieldProp.NextVisible(isFirst))
                {
                    isFirst = false;
                    if (SerializedProperty.EqualContents(fieldProp, endProp)) break;

                    EditorGUILayout.PropertyField(fieldProp, GUIContent.none, GUILayout.Width(GetColumnWidth(colIndex++)));
                }
            }

            if (GUILayout.Button("X", GUILayout.Width(30)))
            {
                targetListProperty.DeleteArrayElementAtIndex(i);
                break;
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        // 💡 새 행 추가 버튼 로직 변경
        if (GUILayout.Button("Add New Row", GUILayout.Height(30)))
        {
            AddNewRow();
        }

        serializedObject.ApplyModifiedProperties();

        HandleColumnResizing();
    }

    // 💡 새 행 추가 및 에셋 자동 생성 로직
    private void AddNewRow()
    {
        targetListProperty.arraySize++;
        int newIndex = targetListProperty.arraySize - 1;
        SerializedProperty newElement = targetListProperty.GetArrayElementAtIndex(newIndex);

        // 추가된 열이 Object Reference 타입일 때만 자동 생성 시도
        if (newElement.propertyType == SerializedPropertyType.ObjectReference)
        {
            System.Type elementType = GetListElementType();

            // 리스트의 타입이 ScriptableObject를 상속받는 타입일 경우
            if (elementType != null && elementType.IsSubclassOf(typeof(ScriptableObject)))
            {
                // 1. 메모리상에 새로운 SO 인스턴스 생성
                ScriptableObject newAsset = ScriptableObject.CreateInstance(elementType);

                // 2. 현재 Target SO가 위치한 폴더 경로 가져오기
                string dbPath = AssetDatabase.GetAssetPath(targetSO);
                string folderPath = string.IsNullOrEmpty(dbPath) ? "Assets" : System.IO.Path.GetDirectoryName(dbPath);

                // 3. 중복되지 않는 고유한 파일명 자동 생성 (예: New QuestData.asset)
                string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/New {elementType.Name}.asset");

                // 4. 프로젝트 폴더에 실제 .asset 파일로 저장
                AssetDatabase.CreateAsset(newAsset, assetPath);
                AssetDatabase.SaveAssets();

                // 5. 새로 만든 에셋을 방금 추가한 리스트 슬롯에 할당
                newElement.objectReferenceValue = newAsset;

                Debug.Log($"[데이터 에디터] 새 에셋이 생성되었습니다: {assetPath}");
            }
            else
            {
                // SO가 아닌 프리팹 등의 참조형일 경우, 이전 요소가 복제되는 것을 막기 위해 슬롯을 비움
                newElement.objectReferenceValue = null;
            }
        }
    }

    // 💡 현재 리스트(배열)가 어떤 클래스/타입을 담고 있는지 리플렉션으로 알아내는 함수
    private System.Type GetListElementType()
    {
        System.Type targetType = targetSO.GetType();
        FieldInfo fieldInfo = targetType.GetField(targetListProperty.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (fieldInfo != null)
        {
            if (fieldInfo.FieldType.IsArray)
            {
                return fieldInfo.FieldType.GetElementType(); // 배열일 경우
            }
            else if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                return fieldInfo.FieldType.GetGenericArguments()[0]; // List<T>일 경우 T 반환
            }
        }
        return null;
    }

    private float GetColumnWidth(int index)
    {
        while (columnWidths.Count <= index) columnWidths.Add(100f);
        return columnWidths[index];
    }

    // (이하 DrawHeader, DrawHeaderCell, HandleColumnResizing, RowMatchesSearch, CheckPropertyContainsSearch, FindListProperty 함수는 이전 코드와 동일합니다.)
    private void DrawHeader()
    {
        if (targetListProperty.arraySize == 0) return;

        GUILayout.BeginHorizontal();
        GUILayout.Label("No.", EditorStyles.boldLabel, GUILayout.Width(30));

        SerializedProperty firstElement = targetListProperty.GetArrayElementAtIndex(0);
        int colIndex = 0;

        if (firstElement.propertyType == SerializedPropertyType.ObjectReference)
        {
            DrawHeaderCell("SO Reference", colIndex++);

            UnityEngine.Object sampleObj = null;
            for (int i = 0; i < targetListProperty.arraySize; i++)
            {
                if (targetListProperty.GetArrayElementAtIndex(i).objectReferenceValue != null)
                {
                    sampleObj = targetListProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                    break;
                }
            }

            if (sampleObj != null)
            {
                SerializedObject sampleSO = new SerializedObject(sampleObj);
                SerializedProperty prop = sampleSO.GetIterator();
                bool enterChildren = true;
                while (prop.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (prop.name == "m_Script") continue;
                    DrawHeaderCell(prop.displayName, colIndex++);
                }
            }
        }
        else
        {
            SerializedProperty fieldProp = firstElement.Copy();
            SerializedProperty endProp = firstElement.GetEndProperty();
            bool isFirst = true;

            while (fieldProp.NextVisible(isFirst))
            {
                isFirst = false;
                if (SerializedProperty.EqualContents(fieldProp, endProp)) break;
                DrawHeaderCell(fieldProp.displayName, colIndex++);
            }
        }

        GUILayout.Label("", GUILayout.Width(30));
        GUILayout.EndHorizontal();
    }

    private void DrawHeaderCell(string label, int colIndex)
    {
        GUILayout.Label(label, EditorStyles.boldLabel, GUILayout.Width(GetColumnWidth(colIndex)));

        Rect headerRect = GUILayoutUtility.GetLastRect();
        Rect resizeRect = new Rect(headerRect.xMax - 3f, headerRect.y, 6f, headerRect.height);

        EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.SplitResizeLeftRight);

        if (Event.current.type == EventType.MouseDown && resizeRect.Contains(Event.current.mousePosition))
        {
            isResizing = true;
            resizingColumnIndex = colIndex;
            Event.current.Use();
        }
    }

    private void HandleColumnResizing()
    {
        if (!isResizing) return;

        if (Event.current.type == EventType.MouseDrag)
        {
            columnWidths[resizingColumnIndex] += Event.current.delta.x;
            columnWidths[resizingColumnIndex] = Mathf.Max(40f, columnWidths[resizingColumnIndex]);
            Repaint();
            Event.current.Use();
        }
        else if (Event.current.rawType == EventType.MouseUp)
        {
            isResizing = false;
        }
    }

    private bool RowMatchesSearch(SerializedProperty rowProp, string query)
    {
        if (string.IsNullOrEmpty(query)) return true;
        string lowerQuery = query.ToLower();

        if (rowProp.propertyType == SerializedPropertyType.ObjectReference)
        {
            if (rowProp.objectReferenceValue != null)
            {
                if (rowProp.objectReferenceValue.name.ToLower().Contains(lowerQuery)) return true;

                SerializedObject innerSO = new SerializedObject(rowProp.objectReferenceValue);
                SerializedProperty innerProp = innerSO.GetIterator();
                bool enter = true;
                while (innerProp.NextVisible(enter))
                {
                    enter = false;
                    if (innerProp.name == "m_Script") continue;
                    if (CheckPropertyContainsSearch(innerProp, lowerQuery)) return true;
                }
            }
        }
        else
        {
            SerializedProperty fieldProp = rowProp.Copy();
            SerializedProperty endProp = rowProp.GetEndProperty();
            bool isFirst = true;
            while (fieldProp.NextVisible(isFirst))
            {
                isFirst = false;
                if (SerializedProperty.EqualContents(fieldProp, endProp)) break;
                if (CheckPropertyContainsSearch(fieldProp, lowerQuery)) return true;
            }
        }
        return false;
    }

    private bool CheckPropertyContainsSearch(SerializedProperty prop, string lowerQuery)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.String:
                return prop.stringValue.ToLower().Contains(lowerQuery);
            case SerializedPropertyType.Integer:
                return prop.intValue.ToString().Contains(lowerQuery);
            case SerializedPropertyType.ObjectReference:
                return prop.objectReferenceValue != null && prop.objectReferenceValue.name.ToLower().Contains(lowerQuery);
        }
        return false;
    }

    private void FindListProperty()
    {
        targetListProperty = null;
        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;
        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (prop.name != "m_Script" && prop.isArray && prop.propertyType != SerializedPropertyType.String)
            {
                targetListProperty = serializedObject.FindProperty(prop.name);
                break;
            }
        }
    }
}
#endif