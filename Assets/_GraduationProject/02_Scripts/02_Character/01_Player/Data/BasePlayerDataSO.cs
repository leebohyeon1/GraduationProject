
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 기본 데이터를 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "BasePlayerDatasSO", menuName = "Player/BasePlayerDatasSO")]
public class BasePlayerDatasSO : ScriptableObject
{
    [Header("Stats")]
    public int MaxHealth = 100; // 최대 체력
    public int MaxHeat = 100;
    public int MaxMana = 3;

    [Header("Movement")]
    public LayerMask GroundLayerMask = 1 << 3; // 지면으로 인식할 레이어 마스크
    public LayerMask ObstacleLayerMask = 1 << 4; // 장애물 레이어 마스크

    public float MoveSpeed = 5f; // 이동 속도
    public float RotateSpeed = 5f; // 회전 속도
    public float Gravity = -9.81f; // 중력 값
    public float GroundCheckDistance = 0.1f; // 지면 체크 거리

    [Header("Combat")]
    public float BattleOutTime = 8f; // 비전투 상태로 전환되는 시간
    public PlayerCombatData CombatData; // 전투 관련 데이터
}


/// <summary>
/// 플레이어의 전투 관련 데이터를 정의하는 구조체입니다.
/// </summary>
[Serializable]
public class PlayerCombatData
{
    [Header("Dodge")]
    public float DodgeSpeed; // 회피 속도
    public float DodgeCooldown; // 회피 쿨타임

    [Header("Damaged")]
    public AnimationCurve KnockbackCurve; // 피격 넉백 애니메이션 커브
    [Range(0f, 1f)]
    public float DefendDamageReductionRate; // 방어 시 데미지 감소율
    public float DefendStaggerDuration; // 방어 시 경직 임계값
    public float DefendKnockbackForce; // 방어 시 넉백 힘

    [Space(10f)]
    public float LightStaggerDuration; // 약한 경직 시간
    public float LightKnockbackForce; // 강한 경직 시간

    // 강한 경직 시 적의 넉백 데이터 받음
    //[Space(10f)]
    //public float HeavyStaggerDuration; // 강한 경직 시간
    //public float HeavyStaggerKnockbackDistance; // 강한 경직 이동 거리

    [Header("Attack")]
    public LayerMask AttackLayerMask; // 공격 시 타겟 레이어 마스크
    public PlayerAttackData[] AttackDatas; // 일반 공격 데이터 배열
    public float LastAttackDelay; // 마지막 공격 후 딜레이

    [Header("ChargeAttack")]
    public PlayerAttackData ChargeAttackData; // 차지 공격 데이터

    [Header("RangedAttack")]
    public RangedAttackData RangedAttackData; // 원거리 공격 데이터

    [Header("Parry")]
    public Vector3 ParryRadius; // 패링 범위
    public float ParryMoveDuration; // 패링 성공 시 이동 거리   
    public float ParryMoveForce; // 패링 성공 시 이동 거리

    [Header("CounterAttack")]
    public float CounterAttackWindow; // 반격 가능 시간
    public PlayerAttackData[] CounterAttackDatas; // 반격 데이터 배열

    /// <summary>
    /// 깊은 복사를 위한 데이터 클론 생성
    /// </summary>
    /// <returns>깊은 복사된 전투 데이터</returns>
    public PlayerCombatData Clone()
    {
        PlayerCombatData newCombatData = new PlayerCombatData
        {
            DodgeSpeed = DodgeSpeed,
            DodgeCooldown = DodgeCooldown,
            KnockbackCurve = KnockbackCurve,

            DefendDamageReductionRate = DefendDamageReductionRate,
            DefendStaggerDuration = DefendStaggerDuration,
            DefendKnockbackForce = DefendKnockbackForce,

            LightStaggerDuration = LightStaggerDuration,
            LightKnockbackForce = LightKnockbackForce,

            AttackLayerMask = AttackLayerMask,
            LastAttackDelay = LastAttackDelay,
            ChargeAttackData = ChargeAttackData,
            RangedAttackData = RangedAttackData,

            ParryRadius = ParryRadius,
            ParryMoveDuration = ParryMoveDuration,
            ParryMoveForce = ParryMoveForce,

            CounterAttackWindow = CounterAttackWindow
        };

        if (AttackDatas != null)
        {
            newCombatData.AttackDatas = new PlayerAttackData[AttackDatas.Length];
            Array.Copy(AttackDatas, newCombatData.AttackDatas, AttackDatas.Length);
        }

        if (CounterAttackDatas != null)
        {
            newCombatData.CounterAttackDatas = new PlayerAttackData[CounterAttackDatas.Length];
            Array.Copy(CounterAttackDatas, newCombatData.CounterAttackDatas, CounterAttackDatas.Length);
        }

        return newCombatData;
    }
}

/// <summary>
/// 플레이어의 공격 데이터를 정의하는 구조체입니다.
/// 공격 시 이동, 데미지, 범위, 딜레이 등을 포함합니다.
/// </summary>
[Serializable]
public struct PlayerAttackData
{
    [Header("Attack Movement")]
    [Tooltip("공격 시 전진하는 거리")]
    public float AttackMoveDistance;

    [Tooltip("공격 이동에 걸리는 시간")]
    public float AttackMoveDuration;

    [Tooltip("공격 이동 애니메이션 커브")]
    public AnimationCurve AttackMoveCurve;

    [Header("Attack Stats")]
    [Tooltip("공격 데미지")]
    public int AttackDamage;

    [Tooltip("공격 범위")]
    public Vector3 AttackRadius;

    [Header("Attack Timing")]
    [Tooltip("공격 후 딜레이")]
    public float AttackDelay;

    [Header("Knockback")]
    public AnimationCurve KnockBackCurve;
    public float KnockBackDuration;
    public float KnockBackForce;
}

/// <summary>
/// 플레이어의 원거리 공격 데이터를 정의하는 구조체입니다.
/// 차지 시간, 발사체 속도, 데미지 등을 포함합니다.
/// </summary>
[Serializable]
public struct RangedAttackData
{
    [Header("Charge Stats")]
    [Tooltip("원거리 공격 차지 시간")]
    public float ChargeTime;

    [Header("Attack Stats")]
    [Tooltip("원거리 공격 데미지")]
    public int AttackDamage;

    [Header("Projectile")]
    public GameObject ProjectilePrefab; // 발사체 프리팹
    [Tooltip("발사체 이동 속도")]
    public float ProjectileSpeed;
    [Tooltip("발사체 이동 애니메이션 커브 (가속/감속 등)")]
    public AnimationCurve ProjectileMoveCurve;

    [Header("Attack Timing")]
    public float AttackDelay; // 공격 후 딜레이
}

