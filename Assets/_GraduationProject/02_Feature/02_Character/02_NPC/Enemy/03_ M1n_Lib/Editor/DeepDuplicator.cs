// --- FILE: DeepDuplicator.cs (in Editor folder) ---

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class DeepDuplicator
{
    // 메뉴 아이템 등록, priority는 메뉴 순서를 의미
    [MenuItem("Assets/Deep Duplicate (Create Folder)", priority = 19)]
    private static void DeepDuplicateAssetWithFolder()
    {
        // 현재 선택된 에셋이 ScriptableObject인지 확인
        ScriptableObject selectedObject = Selection.activeObject as ScriptableObject;
        if (selectedObject == null)
        {
            Debug.LogWarning("Please select a ScriptableObject to duplicate.");
            return;
        }

        // --- 1. 경로 설정 ---
        string originalPath = AssetDatabase.GetAssetPath(selectedObject);
        string originalDirectory = Path.GetDirectoryName(originalPath);
        string newFolderName = selectedObject.name + " (Copy)";
        string newDirectory = Path.Combine(originalDirectory, newFolderName);

        // 만약 같은 이름의 폴더가 이미 존재하면, 고유한 이름으로 변경 (e.g., "Root (Copy) 1")
        newDirectory = AssetDatabase.GenerateUniqueAssetPath(newDirectory);

        // 새로운 폴더 생성
        Directory.CreateDirectory(newDirectory);
        Debug.Log($"Created new directory at: {newDirectory}");

        // --- 2. 재귀적 복제 ---
        // 원본과 복제본의 매핑을 저장할 딕셔너리
        Dictionary<ScriptableObject, ScriptableObject> duplicatedObjects = new Dictionary<ScriptableObject, ScriptableObject>();

        // 메인 에셋부터 시작하여 재귀적으로 모든 참조된 ScriptableObject를 복제
        DuplicateRecursively(selectedObject, duplicatedObjects);

        // --- 3. 복제된 에셋 파일 생성 ---
        foreach (var kvp in duplicatedObjects)
        {
            ScriptableObject originalAsset = kvp.Key;
            ScriptableObject duplicatedAsset = kvp.Value;

            // 새 파일 경로 생성 (새 폴더 안 + 원본 파일 이름 + (Copy))
            string newAssetPath = Path.Combine(newDirectory, originalAsset.name + " (Copy).asset");
            // 혹시 모를 이름 충돌을 위해 고유 경로로 다시 한번 확인
            newAssetPath = AssetDatabase.GenerateUniqueAssetPath(newAssetPath);

            // 에셋 파일 생성
            AssetDatabase.CreateAsset(duplicatedAsset, newAssetPath);
        }

        // 변경사항 저장 및 프로젝트 창 새로고침
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Deep duplication complete! All copies are saved in '{newDirectory}'");
    }

    /// <summary>
    /// ScriptableObject와 그 자식들을 재귀적으로 복제하고 참조를 다시 연결합니다.
    /// </summary>
    private static ScriptableObject DuplicateRecursively(ScriptableObject original, Dictionary<ScriptableObject, ScriptableObject> duplicatedObjects)
    {
        // null이거나 이미 복제된 객체는 건너뛰기
        if (original == null) return null;
        if (duplicatedObjects.ContainsKey(original))
        {
            return duplicatedObjects[original];
        }

        // 원본을 Instantiate하여 메모리상에 복제본 생성
        ScriptableObject copy = Object.Instantiate(original);
        // 복제본 딕셔너리에 추가 (무한 재귀 방지)
        duplicatedObjects.Add(original, copy);

        // SerializedObject를 사용하여 객체의 모든 필드를 순회
        SerializedObject serializedCopy = new SerializedObject(copy);
        SerializedProperty property = serializedCopy.GetIterator();
        
        // 모든 프로퍼티를 검사
        while (property.Next(true))
        {
            // 만약 프로퍼티가 다른 ScriptableObject를 참조하는 경우
            if (property.propertyType == SerializedPropertyType.ObjectReference &&
                property.objectReferenceValue is ScriptableObject referencedSO)
            {
                // 그 참조된 객체도 재귀적으로 복제하고, 그 복제본을 참조하도록 값을 변경
                property.objectReferenceValue = DuplicateRecursively(referencedSO, duplicatedObjects);
            }
        }

        // 변경된 참조 값을 실제 객체에 적용
        serializedCopy.ApplyModifiedProperties();

        return copy;
    }

    // 메뉴가 활성화될 조건을 설정 (ScriptableObject를 선택했을 때만)
    [MenuItem("Assets/Deep Duplicate (Create Folder)", true)]
    private static bool DeepDuplicateValidation()
    {
        return Selection.activeObject is ScriptableObject;
    }
}