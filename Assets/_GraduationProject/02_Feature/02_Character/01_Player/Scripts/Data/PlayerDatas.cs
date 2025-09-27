using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerBaseDatasSO", menuName = "RefactorPlayer/PlayerBaseDatasSO")]
public class PlayerBaseDatasSO : ScriptableObject
{
    [Header("Stats")]
    public int MaxHealth = 100;
    public int MaxMana = 100;

    [Header("Movement")]
    public LayerMask GroundLayerMask = 1 << 3;
    public float MoveSpeed = 5f;
    public float RotateSpeed = 5f;
    public float Gravity = -9.81f;
    public float GroundCheckDistance = 0.1f;

    [Header("Combat")]
    public PlayerCombatData CombatData;
}

/// <summary>
/// 런타임 데이터를 담을 플레이어 데이터 클래스
/// </summary>
[Serializable]
public class PlayerData
{
    public float AnimatorSpeed { get; private set; }
    // State
    public bool IsDefending { get; private set; }
    public bool IsInCombat { get; private set; }
    public bool IsDamaged => IsLightHit || IsHeavyHit;
    public bool IsLightHit { get; private set; } 
    public bool IsHeavyHit { get; private set; } 

    // Stat
    public int MaxHealth;
    public int MaxMana;

    public float MoveSpeed;
    public float RotateSpeed;

    public float BattleOutTime = 8f;
    public PlayerCombatData CombatData = new PlayerCombatData();

    public event Action<float> OnAnimationSpeedChanged;

    public void Initialize(PlayerBaseDatasSO baseData)
    {
        ResetData(baseData);
    }

    public void UpdateData(PlayerBaseDatasSO baseData, TierStatData tierStatData)
    {
        MoveSpeed = baseData.MoveSpeed * tierStatData.SpeedMultiply;
        RotateSpeed = baseData.RotateSpeed * tierStatData.SpeedMultiply;
        AnimatorSpeed = tierStatData.AnimSpeedMultiply;
        OnAnimationSpeedChanged?.Invoke(AnimatorSpeed);

        CombatData.UpdateData(baseData.CombatData, tierStatData);
    }

    public void ResetData(PlayerBaseDatasSO baseData)
    {
        MaxHealth = baseData.MaxHealth;
        MaxMana = baseData.MaxMana;
        MoveSpeed = baseData.MoveSpeed;
        RotateSpeed = baseData.RotateSpeed;
        CombatData.Initialize(baseData.CombatData);
    }

    public void SetDefending(bool defending)
    {
        IsDefending = defending;
    }

    public void SetInCombat(bool inCombat)
    {
        IsInCombat = inCombat;
    }

    public void SetDamaged(PlayerDamagedType damagedType)
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
public class PlayerCombatData
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


    public void Initialize(PlayerCombatData data)
    {
        AttackDatas = new PlayerAttackData[data.AttackDatas.Length];
        ChargeAttackData = new PlayerAttackData();
        CounterAttackDatas = new PlayerAttackData[data.CounterAttackDatas.Length];

        ResetData(data);
    }

    public void ResetData(PlayerCombatData data)
    {
        DodgeSpeed = data.DodgeSpeed;
        DodgeCooldown = data.DodgeCooldown;

        DefendDamageReductionRate = data.DefendDamageReductionRate;
        LightStaggerDuration = data.LightStaggerDuration;
        HeavyStaggerDuration = data.HeavyStaggerDuration;
        AttackLayerMask = data.AttackLayerMask;

        for (int i = 0; i < AttackDatas.Length; i ++)
        {
            AttackDatas[i] = new PlayerAttackData();
            AttackDatas[i].Initialize(data.AttackDatas[i]);
        }

        LastAttackDelay = data.LastAttackDelay;

        ChargeAttackData.Initialize(data.ChargeAttackData);

        RangedAttackData = data.RangedAttackData;

        ParryRadius = data.ParryRadius;

        CounterAttackWindow = data.CounterAttackWindow;
        for (int i = 0; i < CounterAttackDatas.Length; i++)
        {
            CounterAttackDatas[i] = new PlayerAttackData();
            CounterAttackDatas[i].Initialize(data.CounterAttackDatas[i]);
        }
        
    }

    public void UpdateData(PlayerCombatData baseData, TierStatData tierStatData)
    {
        DodgeSpeed = baseData.DodgeSpeed * tierStatData.SpeedMultiply;

        // 공격 데이터 
        for (int i = 0; i < AttackDatas.Length; i++)
        {
            AttackDatas[i].AttackDamage = Mathf.RoundToInt(
                baseData.AttackDatas[i].AttackDamage * tierStatData.DamageMultiply);

            AttackDatas[i].AttackRadius =
                baseData.AttackDatas[i].AttackRadius * tierStatData.RangeMultiply;

            AttackDatas[i].AttackDelay =
                baseData.AttackDatas[i].AttackDelay / tierStatData.AnimSpeedMultiply;
        }

        // 차지 공격 데이터
        ChargeAttackData.AttackDamage = Mathf.RoundToInt(
                baseData.ChargeAttackData.AttackDamage * tierStatData.DamageMultiply);
        
        ChargeAttackData.AttackMoveDistance =
            baseData.ChargeAttackData.AttackMoveDistance * tierStatData.RangeMultiply;
        ChargeAttackData.AttackRadius =
            baseData.ChargeAttackData.AttackRadius * tierStatData.RangeMultiply;

        ChargeAttackData.AttackDelay =
            baseData.ChargeAttackData.AttackDelay / tierStatData.AnimSpeedMultiply;

        // 카운터 공격 데이터
        for (int i = 0; i < CounterAttackDatas.Length; i++)
        {
            CounterAttackDatas[i].AttackDamage = Mathf.RoundToInt(
              baseData.CounterAttackDatas[i].AttackDamage * tierStatData.DamageMultiply);

            CounterAttackDatas[i].AttackRadius =
                baseData.CounterAttackDatas[i].AttackRadius * tierStatData.RangeMultiply;

            CounterAttackDatas[i].AttackDelay =
                baseData.CounterAttackDatas[i].AttackDelay / tierStatData.AnimSpeedMultiply;
        }
    }
}

/// <summary>
/// 플레이어 근접 공격 관련 데이터
/// 공격 시 전진 이동, 공격력, 범위 등을 정의
/// </summary>
[Serializable]
public class PlayerAttackData
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

    public void Initialize(PlayerAttackData attackData)
    {
        AttackMoveDistance = attackData.AttackMoveDistance;
        AttackMoveDuration = attackData.AttackMoveDuration;
        AttackMoveCurve = attackData.AttackMoveCurve;
        AttackDamage = attackData.AttackDamage;
        AttackRadius = attackData.AttackRadius;
        AttackDelay = attackData.AttackDelay;
    }
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


