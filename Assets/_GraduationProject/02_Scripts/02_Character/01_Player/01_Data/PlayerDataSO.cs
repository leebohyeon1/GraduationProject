
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 기본 데이터를 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "PlayerDatasSO", menuName = "Scriptable Objects/Player/PlayerDatasSO")]
public class PlayerDataSO : ScriptableObject
{
    [Header("Health")]
    public int MaxHealth = 100; // 최대 체력
    public int MaxPotion = 3;
    public int PotionHealAmount = 40;

    [Header("Stamina")]
    public float MaxStamina = 100;
    public float StaminaRegenPerSecond = 5;

    [Header("Movement")]
    public float MoveSpeed = 5f; // 이동 속도
    public float MoveAccelerationTime = 0.2f;   // 이동 가속 시간
    public float MoveDecelerationnTime = 0.5f;  // 이동 감속 시간
    public AnimationCurve MoveCurve;            // 이동 속도 곡선

    [Space(10f)]
    public float RotateSpeed = 5f; // 회전 속도
    public float RotateAccelerationTime = 0.2f; // 회전 가속 시간
    public float RotateDecelerationTime = 0.5f; // 회전 감속 시간
    public AnimationCurve RotationCurve;        // 회전 속도 곡선

    [Header("Dodge")]
    public float DodgeStamina = 10;
    public StepData DodgeConfig;            // 회피 설정

    [Header("Combat")]
    public float BattleOutTime = 8f; // 비전투 상태로 전환되는 시간
    public LayerMask AttackLayerMask; // 공격 시 타겟 레이어 마스크

    [Header("NormalAttack Setting")]
    public List<PlayerAttackConfig> NormalAttackConfigList;         // 일반 공격 데이터 배열

    [Header("Charge Setting")]
    public float ChargeMoveSpeed;
    public float ChargeRotationSpeed;
    public float ChargeStamina;
    public float MaxChargeTime = 5f;

    [Header("Counter")]
    public float CounterAngle;
    public StepData CounterKnockbackConfig; // 카운터 성공 시 넉백 설정
    public PlayerAttackConfig NormalCounterAttackConfig;            // 일반 카운터 공격 설정
    public List<PlayerChargeConfig> HeavyCounterAttackConfigList;   // 차징 카운터 공격 설정

    [Header("KnockDown")]
    public float KnockDownDuration = 10;    // 기절 지속시간
}


///// <summary>
///// 플레이어의 전투 관련 데이터를 정의하는 구조체입니다.
///// </summary>
//[Serializable]
//public class PlayerCombatData
//{
//    [Header("Dodge")]
//    public float DodgeStamina = 10;
//    public StepData DodgeConfig;            // 회피 설정

//    [Header("Attack")]
//    public LayerMask AttackLayerMask; // 공격 시 타겟 레이어 마스크
//    public List<PlayerAttackConfig> NormalAttackConfigList;         // 일반 공격 데이터 배열
//    public PlayerAttackConfig NormalCounterAttackConfig;            // 일반 카운터 공격 설정
//    public List<PlayerChargeConfig> HeavyCounterAttackConfigList;   // 차징 카운터 공격 설정

//    [Header("Counter")]
//    public float CounterStamina;
//    public float CounterAngle;
//    public float CounterKnockbackDuration;          // 카운터 성공 시 넉백 시간
//    public float CounterKnockbackDistance;          // 카운터 성공 시 넉백 거리
//    public AnimationCurve CounterKnockbackCurve;    // 카운터 성공 시 넉백 커브

//    [Header("ChargeCounterAttack")]
//    public float ChargeMoveSpeed; 
//    public float ChargeRotateSpeed; 
//    public float ChargeStamina;
//    public float MaxChargeTime = 5f;

//    [Header("KnockDown")]
//    public float KnockDownDuration = 10;    // 기절 지속시간
//}
