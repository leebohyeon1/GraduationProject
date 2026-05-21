using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStat", menuName = "Character/EnemyStat")]
public class EnemyStat : ScriptableObject
{
    public int Maxhealth;
    public float MoveSpeed;
    public float SeeRange;
    public float DetectRange;
    public float CircleSeeRange;
    public int MoneyReward = 10;
    
    public EnemyRewardSO RewardSO;
    public EnemyStateEventSO EStateEventSO;

}