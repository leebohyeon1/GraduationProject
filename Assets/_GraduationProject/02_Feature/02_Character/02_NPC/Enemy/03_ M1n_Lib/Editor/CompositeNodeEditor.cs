using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using BehaviorTree;
// CompositeNode를 상속받는 모든 클래스(Selector, Sequence)에 이 커스텀 에디터를 적용합니다.


[CustomEditor(typeof(CompositeNode), true)] 
public class CompositeNodeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 UI를 먼저 그립니다 (Nodes 배열 등).
        base.OnInspectorGUI();

        // 대상 객체를 CompositeNode로 캐스팅합니다.
        CompositeNode compositeNode = (CompositeNode)target;

        // --- 새 노드 추가 버튼 ---
        EditorGUILayout.Space();
        if (GUILayout.Button("Add New Node"))
        {
            // 노드 타입 선택 창을 띄웁니다.
            NodeTypePicker.Show(selectedType =>
            {
                // 사용자가 타입을 선택했을 때 실행될 로직입니다.
                AddNewNode(compositeNode, selectedType);
            });
        }
    }

    private void AddNewNode(CompositeNode parentNode, Type nodeType)
    {
        // 새 노드 인스턴스를 생성합니다.
        Node newNode = (Node)ScriptableObject.CreateInstance(nodeType);
        newNode.name = nodeType.Name;

        // 현재 부모 노드 에셋이 저장된 경로를 찾습니다.
        string parentPath = AssetDatabase.GetAssetPath(parentNode);
        string directory = Path.GetDirectoryName(parentPath);
        
        // 새 노드를 저장할 경로를 생성합니다 (부모와 같은 폴더).
        // 고유한 파일명을 보장합니다.
        string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, newNode.name + ".asset"));
        
        // 새 노드를 에셋 파일로 저장합니다.
        AssetDatabase.CreateAsset(newNode, assetPath);
        AssetDatabase.SaveAssets();

        // SerializedObject를 사용하여 부모 노드의 "nodes" 배열에 새 노드를 추가합니다.
        // 이렇게 해야 Undo/Redo가 가능하고, 에디터가 변경사항을 올바르게 인식합니다.
        SerializedObject so = new SerializedObject(parentNode);
        SerializedProperty nodesProperty = so.FindProperty("nodes");
        
        // 배열의 크기를 1 늘리고, 마지막 요소에 새 노드를 할당합니다.
        nodesProperty.InsertArrayElementAtIndex(nodesProperty.arraySize);
        nodesProperty.GetArrayElementAtIndex(nodesProperty.arraySize - 1).objectReferenceValue = newNode;

        // 변경사항을 적용합니다.
        so.ApplyModifiedProperties();
        
        Debug.Log($"Added new node '{newNode.name}' to '{parentNode.name}'");
    }
}