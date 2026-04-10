using UnityEngine;
using UnityEditor;

public class TerrainTreeConverter : EditorWindow
{
    [MenuItem("Tools/Terrain Tree Converter/나무 변환 (랜덤 크기 수정판)")]
    public static void Convert()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null) return;

        TerrainData data = terrain.terrainData;
        GameObject treeHolder = new GameObject("Converted_Trees_FixedScale");

        int count = 0;

        foreach (TreeInstance tree in data.treeInstances)
        {
            if (tree.prototypeIndex >= data.treePrototypes.Length) continue;
            GameObject prefab = data.treePrototypes[tree.prototypeIndex].prefab;

            if (prefab == null) continue;

            Vector3 worldPos = Vector3.Scale(tree.position, data.size) + terrain.transform.position;

            GameObject newTree = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            if (newTree != null)
            {
                newTree.transform.position = worldPos;
                newTree.transform.rotation = Quaternion.Euler(0, tree.rotation * Mathf.Rad2Deg, 0);

                // [핵심 수정] 프리팹의 원본 스케일 * 터레인의 랜덤 스케일
                Vector3 originalScale = prefab.transform.localScale;
                newTree.transform.localScale = new Vector3(
                    originalScale.x * tree.widthScale,
                    originalScale.y * tree.heightScale,
                    originalScale.z * tree.widthScale
                );

                newTree.transform.parent = treeHolder.transform;
                count++;
            }
        }

        Debug.Log($"변환 완료: {count}그루 (크기 보정 적용됨)");
        // 편의를 위해 터레인 나무 그리기 끄기
        // terrain.drawTreesAndFoliage = false; 
    }

    [MenuItem("Tools/Terrain Tree Converter/나무 변환 오류 진단")]
    public static void Diagnose()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null) return;

        TerrainData data = terrain.terrainData;
        TreePrototype[] prototypes = data.treePrototypes;

        Debug.Log($"--- 진단 시작: 총 {prototypes.Length}종류의 나무가 있습니다 ---");

        for (int i = 0; i < prototypes.Length; i++)
        {
            TreePrototype treeType = prototypes[i];

            // 프리팹 연결 여부 확인
            if (treeType.prefab == null)
            {
                Debug.LogError($"[오류] {i}번 나무 종류는 변환할 수 없습니다! (프리팹이 비어 있음)");
                // 만약 메쉬 이름이라도 알 수 있다면 출력
                // Debug.LogWarning("이 나무는 Prefab 대신 Mesh만 등록되어 있을 수 있습니다.");
            }
            else
            {
                Debug.Log($"[정상] {i}번 나무 ('{treeType.prefab.name}')는 변환 가능합니다.");
            }
        }
    }

    [MenuItem("Tools/Terrain Tree Converter/터레인 나무만 삭제 (풀은 유지)")]
    public static void RemoveTreesOnly()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("선택된 터레인이 없습니다!");
            return;
        }

        // 실수 방지용 경고창
        if (EditorUtility.DisplayDialog("나무 삭제 경고",
            "현재 터레인(" + terrain.name + ")의 '모든 나무 데이터'를 삭제하시겠습니까?\n\n주의: 이미 게임 오브젝트로 변환된 나무는 삭제되지 않습니다.\n터레인에 심어진 데이터만 삭제됩니다.",
            "네, 삭제합니다", "취소"))
        {
            TerrainData data = terrain.terrainData;

            // 실행 취소(Ctrl+Z) 가능하도록 기록
            Undo.RecordObject(data, "Remove Terrain Trees");

            // [핵심] 나무 데이터만 빈 배열로 초기화 (풀/디테일은 건드리지 않음)
            data.treeInstances = new TreeInstance[0];

            // 변경사항 즉시 적용
            terrain.Flush();

            Debug.Log("터레인의 나무 데이터가 모두 삭제되었습니다. (풀/꽃은 유지됨)");
        }
    }
}