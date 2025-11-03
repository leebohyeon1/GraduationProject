using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(HeatDataManager))]
public class HeatDataManagerEditor : Editor
{
    private bool heatDataBasesFoldout = true;
    private bool tierStatDatabasesFoldout = true;

    public override void OnInspectorGUI()
    {
        // 기본 인스펙터를 그려서 리스트 자체를 관리(추가/삭제)할 수 있게 합니다.
        base.OnInspectorGUI();

        HeatDataManager dataManager = (HeatDataManager)target;

        EditorGUILayout.Space();

        // HeatDataBases 리스트를 위한 Foldout
        heatDataBasesFoldout = EditorGUILayout.Foldout(heatDataBasesFoldout, "Heat DataBases Details", true, EditorStyles.foldoutHeader);
        if (heatDataBasesFoldout && dataManager.SourceMapDataBases != null)
        {
            DrawDatabaseList(dataManager.SourceMapDataBases);
        }

        EditorGUILayout.Space();

        // TierStatDatabases 리스트를 위한 Foldout
        tierStatDatabasesFoldout = EditorGUILayout.Foldout(tierStatDatabasesFoldout, "Tier Stat Databases Details", true, EditorStyles.foldoutHeader);
        if (tierStatDatabasesFoldout && dataManager.TierStatDatabases != null)
        {
            DrawDatabaseList(dataManager.TierStatDatabases);
        }
    }

    // ScriptableObject 리스트를 인스펙터에 그려주는 제네릭 메소드
    private void DrawDatabaseList<T>(List<T> databaseList) where T : ScriptableObject
    {
        if (databaseList.Count == 0)
        {
            // 리스트가 비어있을 경우 메시지를 표시합니다.
            EditorGUILayout.LabelField("  List is empty.");
            return;
        }

        EditorGUI.indentLevel++;
        foreach (T database in databaseList)
        {
            if (database != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // 에셋 이름을 굵은 글씨로 표시합니다.
                EditorGUILayout.LabelField(database.name, EditorStyles.boldLabel);

                // 각 ScriptableObject의 에디터를 생성하고 인스펙터를 그립니다.
                Editor editor = Editor.CreateEditor(database);
                editor.OnInspectorGUI();
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }
        EditorGUI.indentLevel--;
    }
}