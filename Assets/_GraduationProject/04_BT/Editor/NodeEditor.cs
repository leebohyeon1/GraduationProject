// 파일 경로: 03_ M1n_Lib/Editor/NodeEditor.cs

using UnityEditor;
using BehaviorTree; // Node 클래스가 있는 네임스페이스

// CustomEditor 속성의 두 번째 인자(true)는
// Node 클래스를 상속받는 모든 자식 클래스들에게도 이 에디터를 적용하라는 의미입니다.
[CustomEditor(typeof(Node), true)]
public class NodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // GUI에 변경이 있는지 검사를 시작합니다.
        EditorGUI.BeginChangeCheck();

        // 기존과 똑같은 기본 인스펙터를 그려줍니다.
        // 이 함수 덕분에 우리가 직접 변수 필드를 하나하나 그릴 필요가 없습니다.
        DrawDefaultInspector();

        // BeginChangeCheck() 이후로 GUI에 변경이 감지되었다면...
        if (EditorGUI.EndChangeCheck())
        {
            // ...target (현재 인스펙터가 보여주는 Node 객체)을 'dirty'로 표시합니다.
            // 'dirty'는 "변경되어 저장이 필요한 상태"라는 의미의 Unity 내부 용어입니다.
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}