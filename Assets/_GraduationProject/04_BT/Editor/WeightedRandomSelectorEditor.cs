using UnityEditor;
using BehaviorTree;

[CustomEditor(typeof(WeightedRandomSelector))]
public class WeightedRandomSelectorEditor : NodeEditor
{
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        // services 필드를 제외한 모든 프로퍼티를 그립니다.
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "services", "m_Script");
        serializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}
