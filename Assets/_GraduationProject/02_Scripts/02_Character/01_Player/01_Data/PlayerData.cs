using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 런타임 데이터를 관리하는 클래스입니다.
/// 모든 수치는 Stat 객체와 RuntimeWrapper를 통해 관리되어 원본 SO(BaseData)를 완벽하게 보호합니다.
/// </summary>
[System.Serializable]
public class PlayerData
{
    [Header("Static Data (ReadOnly)")]
    private PlayerDataSO _baseData;
    public PlayerDataSO BaseData => _baseData;

    [Header("Basic Info")]
    public int Money;               // 현재 재화
    public int SpecialMoney;        // 현재 특수 재화
    
    public int CurrentHealth;       // 현재 체력
    public float CurrentStamina;    // 현재 스테미나
    public int CurrentPotion;       // 현재 포션 수

    public Vector3 LastPosition;    // 마지막 위치
    public Vector3 RespawnPostion;  // 리스폰 위치

    public List<string> AcquiredAbilityIds = new List<string>();    // 획득한 능력 ID 리스트

    [Header("Runtime Stats (Global 버프 적용용)")]
    public Stat Health;
    public Stat Potion;
    public Stat PotionHealAmount;

    public Stat Stamina;
    public Stat StaminaRegenPerSecond;

    public Stat Regain;       // 모든 공격의 회복 비율

    public Stat MoveSpeed;
    public Stat RotateSpeed;

    public Stat ChargeMoveSpeed;
    public Stat ChargeRotateSpeed;

    public Stat KnockDownDuration;

    [Header("Combat Configuration (Runtime Wrappers)")]
    public List<RuntimeAttackConfig> NormalAttacks = new List<RuntimeAttackConfig>();   // 일반 공격 설정
    public List<RuntimeAttackConfig> HeavyAttacks = new List<RuntimeAttackConfig>();    // 강공격 설정
    
    public RuntimeAttackConfig NormalCounterAttack;         // 일반 상쇄 설정
    public Stat ChargeStamina;                              // 차징 공격 시 소모하는 스테미나
    public Stat MaxChargeTime;                              // 최대 차징 시간
    public RuntimeChargeAttackConfig HeavyCounterAttack;    // 차징 상쇄 설정

    public RuntimeDodgeConfig RuntimeDodge;


    public PlayerData()
    {
        Money = 0;
        AcquiredAbilityIds = new List<string>();
    }

    /// <summary>
    /// PlayerDataSO의 데이터로 초기화합니다.
    /// 값 복사 대신 실시간 참조와 래퍼 클래스를 사용합니다.
    /// </summary>
    public void InitializeFromSO(PlayerDataSO so)
    {
        if (so == null) return;
        _baseData = so;

        // 1. 글로벌 스탯 초기화 (람다 실시간 참조)
        Health = new Stat(() => _baseData.MaxHealth);
        Potion = new Stat(() => _baseData.MaxPotion);
        PotionHealAmount = new Stat(() => _baseData.PotionHealAmount);

        Stamina = new Stat(() => _baseData.MaxStamina);
        StaminaRegenPerSecond = new Stat(() => _baseData.StaminaRegenPerSecond);
        Regain = new Stat(() => 0f);

        MoveSpeed = new Stat(() => _baseData.MoveSpeed);
        RotateSpeed = new Stat(() => _baseData.RotateSpeed);
        ChargeMoveSpeed = new Stat(() => _baseData.ChargeMoveSpeed);
        ChargeRotateSpeed = new Stat(() => _baseData.ChargeRotateSpeed);

        KnockDownDuration = new Stat(() => _baseData.KnockDownDuration);

        // 2. 콤보 공격 데이터 래퍼 초기화 (개별 데미지 버프 가능)
        NormalAttacks.Clear();
        foreach (var config in _baseData.NormalAttackConfigList)
        {
            NormalAttacks.Add(new RuntimeAttackConfig(config));
        }

        HeavyAttacks.Clear();
        foreach (var config in _baseData.HeavyAttackConfigList)
        {
            HeavyAttacks.Add(new RuntimeAttackConfig(config));
        }

        NormalCounterAttack = new RuntimeAttackConfig(_baseData.NormalCounterAttackConfig);
        ChargeStamina = new Stat(() => _baseData.ChargeStamina);
        MaxChargeTime = new Stat(() => _baseData.MaxChargeTime); 
        HeavyCounterAttack = new RuntimeChargeAttackConfig(_baseData.HeavyCounterAttackConfig);

        RuntimeDodge = new RuntimeDodgeConfig(_baseData.DodgeConfig);

        // 3. 단순 수치 및 설정값 (원본 SO 보호를 위해 값 복사)
        CurrentHealth = (int)Health.Value;
        CurrentStamina = Stamina.Value;

        CurrentPotion = (int)Potion.Value;
    }
}
