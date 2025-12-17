
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

    [Header("Stamina")]
    public float MaxStamina = 100;
    public float StaminaRegenPerSecond = 5;

    [Header("Movement")]
    public LayerMask ObstacleLayerMask = 1 << 4; // 장애물 레이어 마스크

    public float MoveSpeed = 5f; // 이동 속도
    public float RotateSpeed = 5f; // 회전 속도

    [Header("Combat")]
    public float BattleOutTime = 8f; // 비전투 상태로 전환되는 시간
    public PlayerCombatData CombatData; // 전투 관련 데이터

    /// <summary>
    /// 데이터 값만 복사
    /// </summary>
    /// <param name="newData"></param>
    public void SetData(PlayerDataSO newData)
    {
        MaxHealth = newData.MaxHealth;
        MaxStamina = newData.MaxStamina;
        StaminaRegenPerSecond = newData.StaminaRegenPerSecond;
        ObstacleLayerMask = newData.ObstacleLayerMask;
        MoveSpeed = newData.MoveSpeed;
        RotateSpeed = newData.RotateSpeed;
        BattleOutTime = newData.BattleOutTime;
        CombatData = newData.CombatData;
    }
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

    [Space(10f)]
    public float MiddleStaggerDuration; // 약한 경직 시간
    public float MiddleKnockbackForce; // 강한 경직 시간

    [Header("Attack")]
    public LayerMask AttackLayerMask; // 공격 시 타겟 레이어 마스크
    public List<PlayerAttackData> AttackDatas; // 일반 공격 데이터 배열

    [Header("ChargeAttack")]
    public float ChargeMoveSpeed; 
    public float ChargeRotateSpeed; 
    public float ChargeStamina;
    public List<PlayerChargeAttackData> ChargeAttackDatas;
    public float MaxChargeTime = 5f;

    [Header("Parry")]
    public float ParryStamina;
    public float ParryAngle;
    public float ParryMoveDuration; // 패링 성공 시 이동 거리   
    public float ParryMoveForce; // 패링 성공 시 이동 거리
}

[Serializable]
public class PlayerChargeAttackData
{
    public float ChargeTime;
    public PlayerAttackData AttackData;
}
