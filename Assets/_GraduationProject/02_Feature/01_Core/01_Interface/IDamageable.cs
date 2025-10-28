using System;
using UnityEngine;

public interface IDamageable
{
    /// <summary>
    /// 현재 체력
    /// </summary>
    public int Health { get; }

    /// <summary>
    /// 최대 체력
    /// </summary>
    public int MaxHealth { get; }

    /// <summary>
    /// 사망 여부
    /// </summary>
    public bool IsDead { get; }

    /// <summary>
    /// 무적 상태 여부
    /// </summary>
    public bool IsInvincible { get; }  


    /// <summary>
    /// 피해를 받는 함수
    /// </summary>
    /// <param name="damageAmount">피해량</param>
    /// <param name="stiffenessAmount">경직도</param>
    /// <param name="heatTier">열기 단계</param>
    public void TakeDamage(int damageAmount,int heatTier, DamageData damageData);

    /// <summary>
    /// 체력 변경 이벤트
    /// </summary>
    public event Action<int, int> OnHealthChanged;

    /// <summary>
    /// 사망 이벤트
    /// </summary>
    public event Action OnDied;
}

[Serializable]
public struct DamageData
{
    public int StiffnessAmount;
    public Transform AttackerTransform;
    public AnimationCurve KnockbackCurve;
    public float KnockbackDuration;
    public float KnockbackForce;
    public DamageData( int stiffnessAmount, Transform attackerTransform, AnimationCurve knockbackCurve = null, float knockbackDuration = 0f, float knockbackForce = 0f)
    {
        StiffnessAmount = stiffnessAmount;
        AttackerTransform = attackerTransform;
        KnockbackCurve = knockbackCurve;
        KnockbackDuration = knockbackDuration;
        KnockbackForce = knockbackForce;
    }
}
