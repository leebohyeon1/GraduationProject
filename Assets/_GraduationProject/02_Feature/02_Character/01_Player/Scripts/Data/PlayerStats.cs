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
    public int MaxHealth; // 최대 체력
    public int CurrentHealth; // 현재 체력

    public int MaxHeat; // 최대 열기
    public int CurrentHeat; // 현재 열기

    public int MaxMana; // 최대 마나
    public int CurrentMana; // 현재 마나

    // Currency
    public int SkillPoint;

    public LayerMask GroundLayerMask = 1 << 3; // 지면 레이어 마스크
    public LayerMask ObstacleLayerMask = 1 << 4; // 장애물 레이어 마스크
    public float Gravity = -9.81f; // 중력
    public float GroundCheckDistance = 0.1f; // 지면과의 거리 체크
    
    public float MoveSpeed; // 이동 속도
    public float RotateSpeed; // 회전 속도

    public float BattleOutTime = 8f; // 비전투 상태로 전환되는 시간
    public PlayerCombatData CombatData = new PlayerCombatData(); // 전투 데이터

    public PlayerSkillData SkillData = new PlayerSkillData(3);

    public float AnimatorSpeed; // 애니메이터 속도
    public event Action<float> OnAnimationSpeedChanged; // 애니메이터 속도 변경 이벤트

    public PlayerStats(PlayerDataBaseSO baseData, PlayerEvents events)
    {
        _dataBase = baseData;
        _events = events;

        _events.OnDataUpdate += UpdateData;

        ResetData();
    }

    public void Dispose()
    {
        _events.OnDataUpdate -= UpdateData;
    }

    /// <summary>
    /// 스탯을 티어에 맞게 업데이트합니다.
    /// </summary>
    public void UpdateData()
    {
        BasePlayerDatasSO baseData = _dataBase.BaseData;
        TierStatData tierStatData = _dataBase.TierStatData.
            GetTierStat(_dataBase.TierStatData.GetCurrentTier(CurrentHeat));

        MoveSpeed = baseData.MoveSpeed * tierStatData.SpeedMultiply;
        RotateSpeed = baseData.RotateSpeed * tierStatData.SpeedMultiply;
        AnimatorSpeed = tierStatData.AnimSpeedMultiply;
        OnAnimationSpeedChanged?.Invoke(AnimatorSpeed);

        UpdateCombatData(tierStatData);
    }

    /// <summary>
    /// 스텟 티어에 맞게 전투 데이터를 업데이트 합니다.
    /// </summary>
    /// <param name="tierStatData">티어 데이터</param>
    private void UpdateCombatData(TierStatData tierStatData)
    {
        BasePlayerDatasSO baseData = _dataBase.BaseData;
        PlayerCombatData combatData = baseData.CombatData;

        CombatData.DodgeSpeed = combatData.DodgeSpeed * tierStatData.SpeedMultiply;

        // 일반 공격
        for(int i = 0; i < CombatData.AttackDatas.Length; i++)
        {
            CombatData.AttackDatas[i].AttackDamage =
                Mathf.RoundToInt(combatData.AttackDatas[i].AttackDamage * tierStatData.DamageMultiply);

            CombatData.AttackDatas[i].AttackRadius =
                combatData.AttackDatas[i].AttackRadius * tierStatData.RangeMultiply;

            CombatData.AttackDatas[i].AttackMoveDuration =
                combatData.AttackDatas[i].AttackMoveDuration / tierStatData.SpeedMultiply;

            CombatData.AttackDatas[i].AttackDelay =
                combatData.AttackDatas[i].AttackDelay / tierStatData.SpeedMultiply;
        }

        CombatData.LastAttackDelay = 
            combatData.LastAttackDelay / tierStatData.SpeedMultiply;

        // 차징 공격
        CombatData.ChargeAttackData.AttackDamage =
             Mathf.RoundToInt(CombatData.ChargeAttackData.AttackDamage * tierStatData.DamageMultiply);

        CombatData.ChargeAttackData.AttackRadius.z =
             combatData.ChargeAttackData.AttackRadius.z * tierStatData.RangeMultiply;

        CombatData.ChargeAttackData.AttackMoveDistance =
            combatData.ChargeAttackData.AttackMoveDistance * tierStatData.RangeMultiply;

        CombatData.ChargeAttackData.AttackMoveDuration =
            combatData.ChargeAttackData.AttackMoveDuration / tierStatData.SpeedMultiply;

        // 원거리 공격
        CombatData.RangedAttackData.AttackDamage =
            Mathf.RoundToInt(combatData.RangedAttackData.AttackDamage * tierStatData.DamageMultiply);


        // 반격 
        for(int i = 0; i < CombatData.CounterAttackDatas.Length; i++)
        {
            CombatData.CounterAttackDatas[i].AttackDamage =
                   Mathf.RoundToInt(combatData.CounterAttackDatas[i].AttackDamage * tierStatData.DamageMultiply);
        }
    }

    /// <summary>
    /// 스탯을 기본 값으로 리셋합니다.
    /// </summary>
    public void ResetData()
    {
        BasePlayerDatasSO baseData = _dataBase.BaseData;

        MaxHealth = baseData.MaxHealth;
        CurrentHealth = MaxHealth;

        MaxHeat = baseData.MaxHeat;
        CurrentHeat = 0;
        
        MaxMana = baseData.MaxMana;
        CurrentMana = 0;

        GroundLayerMask = baseData.GroundLayerMask;
        ObstacleLayerMask = baseData.ObstacleLayerMask;

        Gravity = baseData.Gravity;
        GroundCheckDistance = baseData.GroundCheckDistance;

        MoveSpeed = baseData.MoveSpeed;
        RotateSpeed = baseData.RotateSpeed;

        CombatData = baseData.CombatData.Clone();
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
    [Range(0f,1f)]
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
            DefendDamageReductionRate = DefendDamageReductionRate,
            LightStaggerDuration = LightStaggerDuration,
            HeavyStaggerDuration = HeavyStaggerDuration,
            AttackLayerMask = AttackLayerMask,
            LastAttackDelay = LastAttackDelay,
            ChargeAttackData = ChargeAttackData,
            RangedAttackData = RangedAttackData,
            ParryRadius = ParryRadius,
            CounterAttackWindow = CounterAttackWindow
        };

        if(AttackDatas != null)
        {
            newCombatData.AttackDatas = new PlayerAttackData[AttackDatas.Length];
            Array.Copy(AttackDatas, newCombatData.AttackDatas, AttackDatas.Length);
        }

        if(CounterAttackDatas != null)
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

/// <summary>
/// 플레이어의 스킬 데이터를 정의하는 구조체입니다.
/// </summary>
[Serializable]
public struct PlayerSkillData
{
    public List<bool> IsMainSkillsUnlock;
    public List<List<bool>> IsSubSkillsUnlock;

    public List<float> SkillCoolDown;
    public List<float> SkillCoolDownTimer;

    public List<int> SkillMaxCount;
    public List<int> SkillCount;

    public PlayerSkillData(int count)
    {
        IsMainSkillsUnlock = new List<bool>(count);
        IsSubSkillsUnlock = new List<List<bool>>(count);

        SkillCoolDown = new List<float>(count);
        SkillCoolDownTimer = new List<float>(count);

        SkillMaxCount = new List<int>(count);
        SkillCount = new List<int>(count) {1,1,1 };

    }
}