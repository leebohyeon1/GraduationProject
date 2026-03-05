using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneDatabase", menuName = "Project/Database/Scene Database")]
public class SceneDatabase : ScriptableObject
{
    [Header("게임 내 모든 씬 데이터 모음")]
    // 여기에 우리가 만든 모든 SceneData를 넣어둘 겁니다.
    public List<SceneDataSO> AllScenes = new List<SceneDataSO>();
}