using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 현재 상태와 스탯을 관리하는 클래스입니다.
/// </summary>
[Serializable]
public class PlayerStats: IDisposable
{
    private PlayerEvents _events;
    private PlayerDataBaseSO _dataBase;

    // State
    public bool IsDefending; // 방어중인가?
    public bool IsInvincible; // 무적인가?
    public bool IsCounterAttack; // 반격 가능한가?
    public bool IsLightHit; // 약한 피격중인가?
    public bool IsHeavyHit; // 강한 피격중인가?
    public bool IsDamaged => IsLightHit || IsHeavyHit; // 피격중인가?
    public bool IsOverHeat; // 과열 상태인가?
    public bool IsHeatlock; // 열기 변경이 잠금되었는가?
    public bool IsBoost; // 증폭 상태인가?

    // Stat
    public int CurrentHealth; // 현재 체력
    public int CurrentHeat; // 현재 열기
    public int CurrentMana; // 현재 마나

    // Currency
    public int SkillPoint;
    
    public float MoveSpeed; // 이동 속도
    public float RotateSpeed; // 회전 속도

    // Combat
    public float DodgeSpeed; // 회피 속도

    public PlayerAttackData[] AttackDatas; // 일반 공격 데이터 배열
    public float LastAttackDelay; // 마지막 공격 후 딜레이

    public PlayerAttackData ChargeAttackData; // 차지 공격 데이터

    public RangedAttackData RangedAttackData; // 원거리 공격 데이터

    public PlayerAttackData[] CounterAttackDatas; // 반격 데이터 배열

    // Animation
    public float AnimatorSpeed; // 애니메이터 속도
    public event Action<float> OnAnimationSpeedChanged; // 애니메이터 속도 변경 이벤트

    // Properties
    public BasePlayerDatasSO BasePlayerDatasSO => _dataBase.BaseData;

    public PlayerStats(PlayerDataBaseSO baseData, PlayerEvents events)
    {
        _dataBase = baseData;
        _events = events;
    }

    public void Dispose()
    {
    }
}