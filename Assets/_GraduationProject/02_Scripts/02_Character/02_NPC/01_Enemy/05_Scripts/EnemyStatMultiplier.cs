using System;
using UnityEngine;

/// <summary>
/// 적 능력치 배율
/// </summary>
[Serializable]
public class EnemyStatMultiplier
{
    /// <summary>
    /// 체력 배율
    /// </summary>
    public float HealthMultiply = 1f;
    /// <summary>
    /// 공격력 배율
    /// </summary>
    public float AttackMultiply = 1f;
    /// <summary>
    /// 넉백 거리 배율
    /// </summary>
    public float KnockbackMultiply = 1f;
}
