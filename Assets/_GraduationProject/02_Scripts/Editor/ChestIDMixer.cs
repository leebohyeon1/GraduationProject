using UnityEditor;
using UnityEngine;

public class ChestIDMixer : EditorWindow
{
    [MenuItem("Tools/ChestIDMixer/ID 섞기")]
    public static void Mix()
    {
        Chest[] chests = FindObjectsByType<Chest>(FindObjectsSortMode.None);

        foreach (var chest in chests)
        {
            chest.SetRandomID();
        }

        Debug.Log("Chest ID 섞기 완료!");
    }
}
