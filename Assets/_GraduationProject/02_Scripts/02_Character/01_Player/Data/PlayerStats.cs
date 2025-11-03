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
    // Skill
    public PlayerSkillData SkillData;

    // Animation
    public float AnimatorSpeed; // 애니메이터 속도
    public event Action<float> OnAnimationSpeedChanged; // 애니메이터 속도 변경 이벤트

    // Properties
    public BasePlayerDatasSO BasePlayerDatasSO => _dataBase.BaseData;

    public PlayerStats(PlayerDataBaseSO baseData, PlayerEvents events)
    {
        _dataBase = baseData;
        _events = events;

        _events.OnDataUpdate += UpdateData;

        InitializeData();
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

        DodgeSpeed = combatData.DodgeSpeed * tierStatData.SpeedMultiply;

        // 일반 공격
        for(int i = 0; i < AttackDatas.Length; i++)
        {
            AttackDatas[i].AttackDamage =
                Mathf.RoundToInt(combatData.AttackDatas[i].AttackDamage * tierStatData.DamageMultiply);

            AttackDatas[i].AttackRadius =
                combatData.AttackDatas[i].AttackRadius * tierStatData.RangeMultiply;

            AttackDatas[i].AttackMoveDuration =
                combatData.AttackDatas[i].AttackMoveDuration / tierStatData.SpeedMultiply;

            AttackDatas[i].AttackDelay =
                combatData.AttackDatas[i].AttackDelay / tierStatData.SpeedMultiply;
        }

        LastAttackDelay = 
            combatData.LastAttackDelay / tierStatData.SpeedMultiply;

        float BoostRangeMutiply = IsBoost ? SkillData.BoostRangeMultiply: 1f;
        float BoostDamageMultiply = IsBoost ? SkillData.BoostDamageMultiply : 1f;

        // 차징 공격
        ChargeAttackData.AttackDamage =
             Mathf.RoundToInt(ChargeAttackData.AttackDamage * tierStatData.DamageMultiply * BoostDamageMultiply);

        ChargeAttackData.AttackRadius.z =
             combatData.ChargeAttackData.AttackRadius.z * tierStatData.RangeMultiply * BoostRangeMutiply;

        ChargeAttackData.AttackMoveDistance =
            combatData.ChargeAttackData.AttackMoveDistance * tierStatData.RangeMultiply * BoostRangeMutiply;

        ChargeAttackData.AttackMoveDuration =
            combatData.ChargeAttackData.AttackMoveDuration / tierStatData.SpeedMultiply;

        // 원거리 공격
        RangedAttackData.AttackDamage =
            Mathf.RoundToInt(combatData.RangedAttackData.AttackDamage * tierStatData.DamageMultiply);


        // 반격 
        for(int i = 0; i < CounterAttackDatas.Length; i++)
        {
            CounterAttackDatas[i].AttackDamage =
                   Mathf.RoundToInt(combatData.CounterAttackDatas[i].AttackDamage * tierStatData.DamageMultiply * BoostDamageMultiply);
        }
    }

    /// <summary>
    /// 스탯을 기본 값으로 리셋합니다.
    /// </summary>
    public void InitializeData()
    {
        BasePlayerDatasSO baseData = _dataBase.BaseData;

        CurrentHealth = baseData.MaxHealth;
        CurrentHeat = 0;
        CurrentMana = 0;

        MoveSpeed = baseData.MoveSpeed;
        RotateSpeed = baseData.RotateSpeed;;
        DodgeSpeed = baseData.CombatData.DodgeSpeed;

        AttackDatas = new PlayerAttackData[baseData.CombatData.AttackDatas.Length];
        Array.Copy(baseData.CombatData.AttackDatas, AttackDatas, baseData.CombatData.AttackDatas.Length);
        
        ChargeAttackData = baseData.CombatData.ChargeAttackData;
        RangedAttackData = baseData.CombatData.RangedAttackData;

        CounterAttackDatas = new PlayerAttackData[baseData.CombatData.CounterAttackDatas.Length];
        Array.Copy(baseData.CombatData.CounterAttackDatas, CounterAttackDatas, baseData.CombatData.CounterAttackDatas.Length);

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

/// <summary>
/// 플레이어의 스킬 데이터를 정의하는 구조체입니다.
/// </summary>
[Serializable]
public class PlayerSkillData
{
    public List<bool> IsMainSkillsUnlock = new List<bool>(new bool[3]);
    public List<bool> IsFlashSubSkillsUnlock = new List<bool>(new bool[3]);
    public List<bool> IsBoostSubSkillsUnlock = new List<bool>(new bool[3]);
    public List<bool> IsTimeStopSubSkillsUnlock = new List<bool>(new bool[3]);

    public List<float> SkillCoolDown = new List<float>(new float[3]);
    public List<float> SkillCoolDownTimer = new List<float>(new float[3]);

    public List<int> SkillMaxCount = new List<int>(new int[3]);
    public List<int> SkillCount = new List<int>(new int[3]);

    #region Flash
    [Header("Flash")]
    public bool IsMaxLevelFlash = false;
    #endregion

    #region Boost
    [Header("Boost")]
    public float BoostRangeMultiply = 1f;
    public float BoostDamageMultiply = 1f;
    public bool IsMaxLevelBoost = false;
    #endregion

    public PlayerSkillData(PlayerDataBaseSO dataBase)
    {
        SkillCoolDown[(int)SkillType.Flash] = dataBase.FlashSkill.CoolDown;
        SkillCoolDown[(int)SkillType.Boost] = dataBase.BoostSkill.CoolDown;
        SkillCoolDown[(int)SkillType.TimeStop] = dataBase.TimeStopSkill.CoolDown;

        SkillMaxCount[(int)SkillType.Flash] = dataBase.FlashSkill.Count;
        SkillMaxCount[(int)SkillType.Boost] = dataBase.BoostSkill.Count;
        SkillMaxCount[(int)SkillType.TimeStop] = dataBase.TimeStopSkill.Count;
    }

    #region FlashMethod
    public void SetMaxLevelFlash(bool isMaxLevel)
    {
        IsMaxLevelFlash = isMaxLevel;
    }

    #endregion

    #region BoostMethod
    public void SetBoostRangeMultiply(float amount)
    {
        BoostRangeMultiply = amount;
    }

    public void SetBoostDamageMultiply(float amount)
    {
        BoostDamageMultiply = amount;
    }

    public void SetMaxLevelBoost(bool isMaxLevel)
    {
        IsMaxLevelBoost = isMaxLevel;
    }
    #endregion
}