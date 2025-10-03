using System;
using UnityEngine;

/// <summary>
/// 플레이어의 현재 상태와 스탯을 관리하는 클래스입니다.
/// </summary>
[Serializable]
public class PlayerStats: IDisposable
{
    private PlayerEvents _events;

    // State
    public bool IsDefending; // 방어중인가?
    public bool IsInvincible; // 무적인가?
    public bool IsCounterAttack; // 반격 가능한가?
    public bool IsLightHit; // 약한 피격중인가?
    public bool IsHeavyHit; // 강한 피격중인가?
    public bool IsDamaged => IsLightHit || IsHeavyHit; // 피격중인가?
    public bool IsOverHeat; // 과열 상태인가?
    public bool IsHeatlock; // 열기 변경이 잠금되었는가?

    // Stat
    public int MaxHealth; // 최대 체력
    public int CurrentHealth; // 현재 체력
    public int CurrentHeat; // 현재 열기

    public LayerMask GroundLayerMask = 1 << 3; // 지면 레이어 마스크
    public float Gravity = -9.81f; // 중력
    public float GroundCheckDistance = 0.1f; // 지면과의 거리 체크
    
    public float MoveSpeed; // 이동 속도
    public float RotateSpeed; // 회전 속도

    public float BattleOutTime = 8f; // 비전투 상태로 전환되는 시간
    public PlayerCombatData CombatData = new PlayerCombatData(); // 전투 데이터

    public float AnimatorSpeed; // 애니메이터 속도
    public event Action<float> OnAnimationSpeedChanged; // 애니메이터 속도 변경 이벤트

    public PlayerStats(BasePlayerDatasSO baseData, PlayerEvents events)
    {
        ResetData(baseData);
        _events = events;
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 스탯을 티어에 맞게 업데이트합니다.
    /// </summary>
    /// <param name="baseData">기본 플레이어 데이터</param>
    /// <param name="tierStatData">티어별 스탯 데이터</param>
    public void UpdateData(BasePlayerDatasSO baseData, TierStatData tierStatData)
    {
        MoveSpeed = baseData.MoveSpeed * tierStatData.SpeedMultiply;
        RotateSpeed = baseData.RotateSpeed * tierStatData.SpeedMultiply;
        AnimatorSpeed = tierStatData.AnimSpeedMultiply;
        OnAnimationSpeedChanged?.Invoke(AnimatorSpeed);

        CombatData = baseData.CombatData;
    }

    /// <summary>
    /// 스탯을 기본 값으로 리셋합니다.
    /// </summary>
    /// <param name="baseData">기본 플레이어 데이터</param>
    public void ResetData(BasePlayerDatasSO baseData)
    {
        MaxHealth = baseData.MaxHealth;
        CurrentHealth = MaxHealth;
        CurrentHeat = 0;

        GroundLayerMask = baseData.GroundLayerMask;
        Gravity = baseData.Gravity;
        GroundCheckDistance = baseData.GroundCheckDistance;

        MoveSpeed = baseData.MoveSpeed;
        RotateSpeed = baseData.RotateSpeed;

        CombatData = baseData.CombatData;
    }

    /// <summary>
    /// 피격 타입을 설정합니다.
    /// </summary>
    /// <param name="damagedType">피격 타입</param>
    public void SetDamagedType(PlayerDamagedType damagedType)
    {
        switch (damagedType)
        {
            case PlayerDamagedType.Normal:
                IsLightHit = true;
                break;
            case PlayerDamagedType.Strong:
                IsHeavyHit = true;
                break;
        }
    }

    /// <summary>
    /// 피격 상태를 리셋합니다.
    /// </summary>
    public void ResetDamaged()
    {
        IsLightHit = false;
        IsHeavyHit = false;
    }

    #region EvenetHandler


    #endregion
}

/// <summary>
/// 플레이어의 전투 관련 데이터를 정의하는 구조체입니다.
/// </summary>
[Serializable]
public struct PlayerCombatData
{
    [Header("Dodge")]
    public float DodgeSpeed; // 회피 속도
    public float DodgeCooldown; // 회피 쿨타임

    [Header("Damaged")]
    public float DefendDamageReductionRate; // 방어 시 데미지 감소율
    public float LightStaggerDuration; // 약한 경직 시간
    public float HeavyStaggerDuration; // 강한 경직 시간

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

    [Header("CounterAttack")]
    public float CounterAttackWindow; // 반격 가능 시간
    public PlayerAttackData[] CounterAttackDatas; // 반격 데이터 배열
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