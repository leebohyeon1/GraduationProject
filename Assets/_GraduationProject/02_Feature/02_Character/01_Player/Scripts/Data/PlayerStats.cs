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

    // Combat
    public float BattleOutTime = 8f; // 비전투 상태로 전환되는 시간
    public PlayerCombatData CombatData = new PlayerCombatData(); // 전투 데이터

    // Skill
    public PlayerSkillData SkillData;

    // Animation
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

        float BoostRangeMutiply = IsBoost ? SkillData.BoostRangeMultiply: 1f;
        float BoostDamageMultiply = IsBoost ? SkillData.BoostDamageMultiply : 1f;

        // 차징 공격
        CombatData.ChargeAttackData.AttackDamage =
             Mathf.RoundToInt(CombatData.ChargeAttackData.AttackDamage * tierStatData.DamageMultiply * BoostDamageMultiply);

        CombatData.ChargeAttackData.AttackRadius.z =
             combatData.ChargeAttackData.AttackRadius.z * tierStatData.RangeMultiply * BoostRangeMutiply;

        CombatData.ChargeAttackData.AttackMoveDistance =
            combatData.ChargeAttackData.AttackMoveDistance * tierStatData.RangeMultiply * BoostRangeMutiply;

        CombatData.ChargeAttackData.AttackMoveDuration =
            combatData.ChargeAttackData.AttackMoveDuration / tierStatData.SpeedMultiply;

        // 원거리 공격
        CombatData.RangedAttackData.AttackDamage =
            Mathf.RoundToInt(combatData.RangedAttackData.AttackDamage * tierStatData.DamageMultiply);


        // 반격 
        for(int i = 0; i < CombatData.CounterAttackDatas.Length; i++)
        {
            CombatData.CounterAttackDatas[i].AttackDamage =
                   Mathf.RoundToInt(combatData.CounterAttackDatas[i].AttackDamage * tierStatData.DamageMultiply * BoostDamageMultiply);
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

        SkillData = new PlayerSkillData(_dataBase);


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

