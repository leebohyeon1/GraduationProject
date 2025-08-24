using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// 이 네임스페이스가 없다면 BehaviorTree를 포함하도록 수정하세요.
using BehaviorTree; 

public class ReferenceRelinker
{
    // 메뉴 아이템 등록
    [MenuItem("Assets/Relink Behavior Tree References", priority = 20)]
    private static void RelinkReferences()
    {
        // 선택된 모든 Node 에셋을 가져옵니다.
        var selectedNodes = Selection.GetFiltered<Node>(SelectionMode.Assets);
        if (selectedNodes.Length == 0)
        {
            Debug.LogWarning("Please select Behavior Tree Node asset(s) to relink.");
            return;
        }

        // 선택된 각 노드에 대해 재연결 작업을 수행합니다.
        foreach (var rootNode in selectedNodes)
        {
            string rootPath = AssetDatabase.GetAssetPath(rootNode);
            string directory = Path.GetDirectoryName(rootPath);

            // 해당 폴더 내의 모든 Node 에셋을 미리 로드하여 딕셔너리에 저장합니다.
            // 이렇게 하면 파일 검색 속도가 빨라집니다.
            var allNodesInFolder = new Dictionary<string, Node>();
            string[] guids = AssetDatabase.FindAssets("t:Node", new[] { directory });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Node nodeAsset = AssetDatabase.LoadAssetAtPath<Node>(path);
                if (nodeAsset != null && !allNodesInFolder.ContainsKey(nodeAsset.name))
                {
                    allNodesInFolder.Add(nodeAsset.name, nodeAsset);
                }
            }
            
            Debug.Log($"Starting relink for '{rootNode.name}'. Found {allNodesInFolder.Count} nodes in the folder '{directory}'.");

            // 재귀적으로 참조를 재연결합니다.
            RelinkNodeRecursively(rootNode, allNodesInFolder, new HashSet<Node>());
            
            // 변경된 에셋을 저장합니다.
            EditorUtility.SetDirty(rootNode);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Relinking process finished for all selected nodes.");
    }

    private static void RelinkNodeRecursively(Node currentNode, Dictionary<string, Node> nodeMap, HashSet<Node> processedNodes)
    {
        // 이미 처리한 노드는 건너뛰어 무한 루프를 방지합니다.
        if (currentNode == null || processedNodes.Contains(currentNode))
        {
            return;
        }
        processedNodes.Add(currentNode);
        
        // SerializedObject를 사용하여 모든 필드를 검사합니다.
        SerializedObject serializedNode = new SerializedObject(currentNode);
        SerializedProperty property = serializedNode.GetIterator();
        bool needsUpdate = false;

        while (property.Next(true))
        {
            // 객체 참조 필드만 확인합니다.
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                // 참조가 비어있거나(None) Node 타입이 아니면 건너뜁니다.
                if (property.objectReferenceValue == null || !(property.objectReferenceValue is Node))
                {
                    continue;
                }

                Node referencedNode = property.objectReferenceValue as Node;
                string originalName = referencedNode.name;

                // 맵에 동일한 이름의 노드가 있고, 그 노드가 현재 참조와 다른 인스턴스일 경우
                if (nodeMap.ContainsKey(originalName) && nodeMap[originalName] != referencedNode)
                {
                    // 참조를 폴더 내에서 찾은 올바른 노드로 교체합니다.
                    property.objectReferenceValue = nodeMap[originalName];
                    needsUpdate = true;
                    Debug.Log($"Relinked property '{property.displayName}' in '{currentNode.name}' to new asset '{nodeMap[originalName].name}'");
                }
                
                // 참조된 자식 노드에 대해서도 재귀적으로 작업을 수행합니다.
                RelinkNodeRecursively(property.objectReferenceValue as Node, nodeMap, processedNodes);
            }
        }

        if (needsUpdate)
        {
            serializedNode.ApplyModifiedProperties();
        }
    }
}