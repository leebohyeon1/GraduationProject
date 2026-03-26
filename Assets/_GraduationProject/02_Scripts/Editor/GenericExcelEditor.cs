#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

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

    // Styles
    private GUIStyle headerStyle;
    private GUIStyle rowStyle;
    private GUIStyle toolbarSearchStyle;
    private GUIStyle toolbarSearchCancelStyle;

    [MenuItem("Tools/Universal Data Editor")]
    public static void ShowWindow()
    {
        GenericExcelEditor window = GetWindow<GenericExcelEditor>("범용 데이터 에디터");
        window.minSize = new Vector2(600, 400);
    }

    private void OnEnable()
    {
        InitStyles();
    }

    private void InitStyles()
    {
        headerStyle = new GUIStyle(EditorStyles.toolbarButton);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleLeft;

        rowStyle = new GUIStyle();
        rowStyle.padding = new RectOffset(2, 2, 2, 2);

        toolbarSearchStyle = GUI.skin.FindStyle("ToolbarSearchTextField");
        toolbarSearchCancelStyle = GUI.skin.FindStyle("ToolbarSearchCancelButton");
    }

    private void OnGUI()
    {
        if (headerStyle == null) InitStyles();

        DrawTopToolbar();

        // 💡 자동 복구 로직: targetSO는 있는데 내부 참조가 깨진 경우 재연결
        if (targetSO != null && (serializedObject == null || targetListProperty == null || serializedObject.targetObject != targetSO))
        {
            serializedObject = new SerializedObject(targetSO);
            FindListProperty();
        }

        if (targetSO == null || targetListProperty == null)
        {
            DrawEmptyState();
            return;
        }

        serializedObject.Update();

        // 1. 고정 헤더 (수평 스크롤만 본문과 동기화)
        EditorGUILayout.BeginHorizontal();
        float currentScrollX = scrollPos.x;
        scrollPos.x = EditorGUILayout.BeginScrollView(new Vector2(currentScrollX, 0), GUIStyle.none, GUIStyle.none, GUILayout.Height(25)).x;
        DrawHeader();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();

        // 2. 메인 바디 (수평/수직 스크롤)
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        DrawBody();
        EditorGUILayout.EndScrollView();

        // 3. 푸터
        DrawFooter();

        serializedObject.ApplyModifiedProperties();
        HandleColumnResizing();

        if (isResizing) Repaint();
    }

    private void DrawTopToolbar()
    {
        // 💡 1단: 데이터 선택 및 추가 버튼
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        EditorGUI.BeginChangeCheck();
        targetSO = (ScriptableObject)EditorGUILayout.ObjectField(GUIContent.none, targetSO, typeof(ScriptableObject), false, GUILayout.Width(300));
        if (EditorGUI.EndChangeCheck())
        {
            if (targetSO != null)
            {
                serializedObject = new SerializedObject(targetSO);
                FindListProperty();
                columnWidths.Clear();
            }
            else
            {
                serializedObject = null;
                targetListProperty = null;
            }
        }

        GUILayout.FlexibleSpace();

        if (targetListProperty != null)
        {
            if (GUILayout.Button(new GUIContent(" Add Row", EditorGUIUtility.IconContent("Toolbar Plus").image), EditorStyles.toolbarButton, GUILayout.Width(90)))
            {
                AddNewRow();
            }
        }

        EditorGUILayout.EndHorizontal();

        // 💡 2단: 전용 검색바 (더 잘 보이게 별도 행으로 분리)
        if (targetListProperty != null)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(5);
            
            // 돋보기 아이콘과 레이블
            GUILayout.Label(EditorGUIUtility.IconContent("d_SearchIcon"), GUILayout.Width(20));
            GUILayout.Label("Search Filter:", EditorStyles.miniLabel, GUILayout.Width(75));
            
            // 전용 검색 텍스트 필드
            searchQuery = EditorGUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField);
            
            // 검색 지우기 버튼 (X 아이콘)
            if (!string.IsNullOrEmpty(searchQuery))
            {
                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton")))
                {
                    searchQuery = "";
                    GUI.FocusControl(null);
                }
            }
            
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawEmptyState()
    {
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        string message = targetSO == null ? "데이터가 담긴 ScriptableObject를 여기에 드래그 하세요." : "이 ScriptableObject에는 편집 가능한 리스트나 배열이 없습니다.";
        EditorGUILayout.HelpBox(message, MessageType.Info);
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
    }

    private void DrawHeader()
    {
        if (targetListProperty == null) return;

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("No.", headerStyle, GUILayout.Width(35));

        int colIndex = 0;
        SerializedProperty firstElement = null;
        if (targetListProperty.arraySize > 0)
            firstElement = targetListProperty.GetArrayElementAtIndex(0);

        if (firstElement != null)
        {
            if (firstElement.propertyType == SerializedPropertyType.ObjectReference)
            {
                DrawHeaderCell("SO Reference", colIndex++);

                UnityEngine.Object sampleObj = null;
                for (int i = 0; i < targetListProperty.arraySize; i++)
                {
                    var val = targetListProperty.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (val != null) { sampleObj = val; break; }
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
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawHeaderCell(string label, int colIndex)
    {
        float width = GetColumnWidth(colIndex);
        GUILayout.Label(label, headerStyle, GUILayout.Width(width));

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

    private void DrawBody()
    {
        for (int i = 0; i < targetListProperty.arraySize; i++)
        {
            SerializedProperty rowProp = targetListProperty.GetArrayElementAtIndex(i);
            if (!RowMatchesSearch(rowProp, searchQuery)) continue;

            Rect rect = EditorGUILayout.BeginHorizontal(rowStyle);
            
            // Zebra Striping (홀수 행 배경색 강조)
            if (i % 2 == 0)
                EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.03f));

            GUILayout.Label(i.ToString(), EditorStyles.miniLabel, GUILayout.Width(35));

            int colIndex = 0;
            if (rowProp.propertyType == SerializedPropertyType.ObjectReference)
            {
                // 💡 false를 추가하여 리스트가 내부에서 펼쳐지는 것을 방지
                EditorGUILayout.PropertyField(rowProp, GUIContent.none, false, GUILayout.Width(GetColumnWidth(colIndex++)));
                if (rowProp.objectReferenceValue != null)
                {
                    SerializedObject innerSO = new SerializedObject(rowProp.objectReferenceValue);
                    innerSO.Update();
                    SerializedProperty innerProp = innerSO.GetIterator();
                    bool enter = true;
                    while (innerProp.NextVisible(enter))
                    {
                        enter = false;
                        if (innerProp.name == "m_Script") continue;
                        // 💡 여기서도 false를 추가하여 하위 리스트 확장을 막음
                        EditorGUILayout.PropertyField(innerProp, GUIContent.none, false, GUILayout.Width(GetColumnWidth(colIndex++)));
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
                    // 💡 여기서도 false를 추가
                    EditorGUILayout.PropertyField(fieldProp, GUIContent.none, false, GUILayout.Width(GetColumnWidth(colIndex++)));
                }
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash"), GUILayout.Width(30), GUILayout.Height(18)))
            {
                if (EditorUtility.DisplayDialog("데이터 삭제", $"{i}번 항목을 삭제하시겠습니까?", "삭제", "취소"))
                {
                    targetListProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawFooter()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        int total = targetListProperty.arraySize;
        int filtered = GetFilteredCount();
        GUILayout.Label($"Total: {total} | Filtered: {filtered}", EditorStyles.miniLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Scroll to Top", EditorStyles.toolbarButton))
        {
            scrollPos = Vector2.zero;
        }
        EditorGUILayout.EndHorizontal();
    }

    private int GetFilteredCount()
    {
        if (string.IsNullOrEmpty(searchQuery)) return targetListProperty.arraySize;
        int count = 0;
        for (int i = 0; i < targetListProperty.arraySize; i++)
        {
            if (RowMatchesSearch(targetListProperty.GetArrayElementAtIndex(i), searchQuery)) count++;
        }
        return count;
    }

    private void AddNewRow()
    {
        serializedObject.Update(); // 현재 상태 동기화

        int newIndex = targetListProperty.arraySize;
        targetListProperty.InsertArrayElementAtIndex(newIndex);
        SerializedProperty newElement = targetListProperty.GetArrayElementAtIndex(newIndex);

        if (newElement.propertyType == SerializedPropertyType.ObjectReference)
        {
            System.Type elementType = GetListElementType();
            if (elementType != null && elementType.IsSubclassOf(typeof(ScriptableObject)))
            {
                ScriptableObject newAsset = ScriptableObject.CreateInstance(elementType);
                string dbPath = AssetDatabase.GetAssetPath(targetSO);
                string folderPath = string.IsNullOrEmpty(dbPath) ? "Assets" : System.IO.Path.GetDirectoryName(dbPath);
                string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/New {elementType.Name}.asset");
                
                AssetDatabase.CreateAsset(newAsset, assetPath);
                AssetDatabase.SaveAssets();
                
                newElement.objectReferenceValue = newAsset;
            }
            else
            {
                newElement.objectReferenceValue = null;
            }
        }

        serializedObject.ApplyModifiedProperties(); // 💡 변경사항 즉시 적용
        AssetDatabase.SaveAssets(); // SO 파일 저장
    }

    private System.Type GetListElementType()
    {
        if (targetSO == null) return null;
        System.Type targetType = targetSO.GetType();
        FieldInfo fieldInfo = targetType.GetField(targetListProperty.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (fieldInfo != null)
        {
            if (fieldInfo.FieldType.IsArray) return fieldInfo.FieldType.GetElementType();
            else if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                return fieldInfo.FieldType.GetGenericArguments()[0];
        }
        return null;
    }

    private float GetColumnWidth(int index)
    {
        while (columnWidths.Count <= index) columnWidths.Add(120f);
        return columnWidths[index];
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
            case SerializedPropertyType.Float:
                return prop.floatValue.ToString().Contains(lowerQuery);
            case SerializedPropertyType.Enum:
                return prop.enumDisplayNames[prop.enumValueIndex].ToLower().Contains(lowerQuery);
            case SerializedPropertyType.ObjectReference:
                return prop.objectReferenceValue != null && prop.objectReferenceValue.name.ToLower().Contains(lowerQuery);
        }
        return false;
    }

    private void FindListProperty()
    {
        targetListProperty = null;
        if (targetSO == null) return;
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
