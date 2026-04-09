using System;
using UnityEngine;

public interface IDamageable
{
    /// <summary>
    /// 현재 체력
    /// </summary>
    public int CurrentHealth { get; }

    /// <summary>
    /// 최대 체력
    /// </summary>
    public int MaxHealth { get; }

    /// <summary>
    /// 사망 여부
    /// </summary>
    public bool IsDead { get; }

    /// <summary>
    /// 피해를 받는 함수
    /// </summary>
    public void TakeDamage(DamageData damageData);

    /// <summary>
    /// 체력 변경 이벤트
    /// </summary>
    public event Action<int, int> OnHealthChanged;

    /// <summary>
    /// 사망 이벤트
    /// </summary>
    public event Action OnDied;
    public bool invincibility { get; set; }
}

/// <summary>
/// 데미지 데이터
/// </summary>
[Serializable]
public struct DamageData
{
    public Transform AttackerTransform;
    public AttackType AttackType;
    public int DamageAmount;
    public int StiffnessAmount;
    
    public AnimationCurve KnockbackCurve;
    public float KnockbackDuration;
    public float KnockbackForce;

    public bool IsMagic;

    public DamageData(Transform attackerTransform, AttackType attackType, int damageAmount, int stiffnessAmount, 
        AnimationCurve knockbackCurve = null, float knockbackDuration = 0f, float knockbackForce = 0f, bool isMagic = false)
    {
        AttackerTransform = attackerTransform;
        AttackType = attackType;
        DamageAmount = damageAmount;    
        StiffnessAmount = stiffnessAmount;
        KnockbackCurve = knockbackCurve;
        KnockbackDuration = knockbackDuration;
        KnockbackForce = knockbackForce;
        IsMagic = isMagic;
    }
}

[Serializable]
public enum AttackType
{
    Normal_0,
    Normal_1,
    Normal_2,
    Normal_3,
    Strong_1,
    Strong_2,
    Strong_3,
    Normal_Counter,
    Strong_Counter,
    Absoluteness,
}
