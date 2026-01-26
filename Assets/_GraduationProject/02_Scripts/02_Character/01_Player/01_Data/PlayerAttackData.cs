using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerChargeConfig
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

    [Header("Attack Movement")]
    public StepData AttackMoveConfig;           // 공격 이동 설정

    [Header("Knockback")]
    public StepData KnockbackCofig;        // 넉백 설정
}