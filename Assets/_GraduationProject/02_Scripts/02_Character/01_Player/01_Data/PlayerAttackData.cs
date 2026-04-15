using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PlayerChargeConfig
{
    public float ChargeTime;
    public PlayerAttackConfig AttackConfig;
}

[Serializable]
public struct PlayerAttackConfig
{
    [Header("Attack Stats")]
    public AttackType AttackType;       // 공격 타입
    public float AttackStamina;         // 공격 스테미나 양
    public Vector3 AttackRadius;        // 공격 범위
    public int AttackDamage;            // 공격 데미지
    public float RegainRate;           // 공격 회복 비율

    [Header("Attack Movement")]
    public StepData AttackMoveConfig;           // 공격 이동 설정

    [Header("Knockback")]
    public StepData KnockbackConfig;        // 넉백 설정
    public StepData DeathKnockbackConfig;   // 사망 넉백 설정
}