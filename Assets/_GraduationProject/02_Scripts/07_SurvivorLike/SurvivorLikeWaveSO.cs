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

    public SurvivorLikeWaveSO GetNextWave()
    {
        return NextWaves[UnityEngine.Random.Range(0, NextWaves.Count - 1)];
    }
}

/// <summary>
/// 웨이브에 소환되는 적 종류
/// </summary>
[Serializable]
public struct SurvivorLikeWaveEntry
{
    public GameObject EnemyPrefab;
    public int EnemyCount;

    public EnemyStatMultiplier StatMultiplier;
}