using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

/// <summary>
/// 씬에 배치된 상자(Chest)와 몬스터(MonsterSavePersistence)의 고유 ID를 
/// 일괄적으로 생성하거나 섞어주는 에디터 툴입니다.
/// </summary>
public class SaveIDMixer : EditorWindow
{
    [MenuItem("Tools/SaveIDMixer/모든 ID 섞기 (상자 & 몬스터)")]
    public static void MixAll()
    {
        MixChests();
        MixMonsters();
        Debug.Log("<color=green>모든 상자와 몬스터의 ID를 성공적으로 갱신했습니다!</color>");
    }

    [MenuItem("Tools/SaveIDMixer/상자 ID만 섞기")]
    public static void MixChests()
    {
        Chest[] chests = FindObjectsByType<Chest>(FindObjectsSortMode.None);
        int count = 0;

        foreach (var chest in chests)
        {
            chest.SetRandomID();
            count++;
        }

        if (count > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"상자 {count}개의 ID 섞기 완료!");
        }
    }

    [MenuItem("Tools/SaveIDMixer/몬스터 ID만 섞기")]
    public static void MixMonsters()
    {
        MonsterSavePersistence[] monsters = FindObjectsByType<MonsterSavePersistence>(FindObjectsSortMode.None);
        int count = 0;

        foreach (var monster in monsters)
        {
            monster.SetRandomID();
            count++;
        }

        if (count > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"몬스터 {count}개의 ID 섞기 완료!");
        }
    }
}
