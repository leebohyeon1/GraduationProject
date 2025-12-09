using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SurvivorLikeWaveSO", menuName = "SurvivorLike/SurvivorLikeWaveSO")]
public class SurvivorLikeWaveSO : ScriptableObject
{
    [Header("Wave Entry")]
    public List<SurvivorLikeWaveEntry> Entries; 

    [Header("Next Waves")]
    public List<SurvivorLikeWaveSO> NextWaves;
}

[Serializable]
public struct SurvivorLikeWaveEntry
{
    public GameObject EnemyPrefab;
    public int EnemyCount;
}