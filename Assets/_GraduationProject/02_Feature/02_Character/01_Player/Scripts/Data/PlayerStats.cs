using System;
using UnityEngine;

/// <summary>
/// 런타임 데이터를 담을 플레이어 데이터 클래스
/// </summary>
[Serializable]
public class PlayerStats
{

    // State
    public bool IsDefending;
    public bool IsInvincible;
    public bool IsCounterAttack;
    public bool IsLightHit;
    public bool IsHeavyHit;
    public bool IsDamaged => IsLightHit || IsHeavyHit;
    public bool IsOverHeat;

    // Stat
    public int MaxHealth;
    public int CurrentHealth;
    public int CurrentHeat;

    public LayerMask GroundLayerMask = 1 << 3;
    public float Gravity = -9.81f;
    public float GroundCheckDistance = 0.1f;
    
    public float MoveSpeed;
    public float RotateSpeed;

    public float BattleOutTime = 8f;
    public PlayerCombatData CombatData = new PlayerCombatData();

    public float AnimatorSpeed;
    public event Action<float> OnAnimationSpeedChanged;

    public PlayerStats(BasePlayerDatasSO baseData)
    {
        ResetData(baseData);
    }

    public void UpdateData(BasePlayerDatasSO baseData, TierStatData tierStatData)
    {
        MoveSpeed = baseData.MoveSpeed * tierStatData.SpeedMultiply;
        RotateSpeed = baseData.RotateSpeed * tierStatData.SpeedMultiply;
        AnimatorSpeed = tierStatData.AnimSpeedMultiply;
        OnAnimationSpeedChanged?.Invoke(AnimatorSpeed);

        CombatData = baseData.CombatData;
    }

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

    public void ResetDamaged()
    {
        IsLightHit = false;
        IsHeavyHit = false;
    }
}

/// <summary>
/// 플레이어 전투관련한 데이터
/// </summary>
[Serializable]
public struct PlayerCombatData
{
    [Header("Dodge")]
    public float DodgeSpeed;
    public float DodgeCooldown;

    [Header("Damaged")]
    public float DefendDamageReductionRate;
    public float LightStaggerDuration;
    public float HeavyStaggerDuration;

    [Header("Attack")]
    public LayerMask AttackLayerMask;
    public PlayerAttackData[] AttackDatas;
    public float LastAttackDelay;

    [Header("ChargeAttack")]
    public PlayerAttackData ChargeAttackData;

    [Header("RangedAttack")]
    public RangedAttackData RangedAttackData;

    [Header("Parry")]
    public Vector3 ParryRadius;

    [Header("CounterAttack")]
    public float CounterAttackWindow;
    public PlayerAttackData[] CounterAttackDatas;
}

/// <summary>
/// 플레이어 근접 공격 관련 데이터
/// 공격 시 전진 이동, 공격력, 범위 등을 정의
/// </summary>
[Serializable]
public struct PlayerAttackData
{
    [Header("Attack Movement")]
    [Tooltip("공격 시 전진할 거리")]
    public float AttackMoveDistance;

    [Tooltip("전진 이동 지속 시간")]
    public float AttackMoveDuration;

    [Tooltip("전진 이동 애니메이션 곡선")]
    public AnimationCurve AttackMoveCurve;

    [Header("Attack Stats")]
    [Tooltip("공격 데미지")]
    public int AttackDamage;

    [Tooltip("공격 범위 반지름")]
    public Vector3 AttackRadius;

    [Header("Attack Timing")]
    [Tooltip("공격후 딜레이 시간")]
    public float AttackDelay;
}

/// <summary>
/// 플레이어 원거리 공격 관련 데이터
/// 차징 시간, 투사체 속도, 데미지 등을 정의
/// </summary>
[Serializable]
public struct RangedAttackData
{
    [Header("Charge Stats")]
    [Tooltip("원거리 공격 차징 시간")]
    public float ChargeTime;

    [Header("Attack Stats")]
    [Tooltip("원거리 공격 데미지")]
    public int AttackDamage;

    [Header("Projectile")]
    public GameObject ProjectilePrefab;
    [Tooltip("투사체 이동 속도")]
    public float ProjectileSpeed;
    [Tooltip("투사체 이동 애니메이션 곡선 (현재 미사용)")]
    public AnimationCurve ProjectileMoveCurve;

    [Header("Attack Timing")]
    public float AttackDelay;
}


