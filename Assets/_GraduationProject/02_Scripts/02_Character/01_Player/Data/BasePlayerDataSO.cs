
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 기본 데이터를 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "PlayerDatasSO", menuName = "Player/PlayerDatasSO")]
public class PlayerDataSO : ScriptableObject
{
    [Header("Health")]
    public int MaxHealth = 100; // 최대 체력

    [Header("Stamina")]
    public float MaxStamina = 100;
    public float StaminaRegenPerSecond = 5;

    [Header("Mana")]
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
    public float DodgeStamina = 10;
    public float DodgeDistance; // 회피 거리
    public float DodgeDuration; 
    public float DodgeRotateSpeed; // 회피 쿨타임
    public AnimationCurve DodgeAnimationCurve;

    [Header("Damaged")]
    public AnimationCurve KnockbackCurve; // 피격 넉백 애니메이션 커브
    [Range(0f, 1f)]
    public float DefendDamageReductionRate; // 방어 시 데미지 감소율
    public float DefendStaggerDuration; // 방어 시 경직 임계값
    public float DefendKnockbackForce; // 방어 시 넉백 힘

    [Space(10f)]
    public float LightStaggerDuration; // 약한 경직 시간
    public float LightKnockbackForce; // 강한 경직 시간

    [Header("Attack")]
    public LayerMask AttackLayerMask; // 공격 시 타겟 레이어 마스크
    public PlayerAttackDataSO[] AttackDatas; // 일반 공격 데이터 배열

    [Header("ChargeAttack")]
    public float ChargeDuration;
    public PlayerAttackDataSO ChargeAttackData; // 차지 공격 데이터

    [Header("Parry")]
    public float ParryStamina;
    public Vector3 ParryRadius; // 패링 범위
    public float ParryMoveDuration; // 패링 성공 시 이동 거리   
    public float ParryMoveForce; // 패링 성공 시 이동 거리
}
