using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [Header("Basic Info")]
    public int Money;
    public int SpecialMoney;
    
    [Header("Stats")]
    public int CurrentHealth;
    public int MaxHealth;
    public float CurrentStamina;
    public float MaxStamina;
    
    [Header("Combat Stats")]
    public float AttackDamageMultiplier;
    public float AttackRegainRate;
    public float PlusNormalAttackSpeedMultiplier;
    public float MaxNormalAttackSpeedMultiplier;
    public List<float> ParryStackDamageMultipliers; // 스택별 데미지 배율
    public string CurrentSpecialAttackId;
    
    [Header("Combat Config")]
    public float ChargeStamina;
    public float MaxChargeTime;
    public float CounterAngle;
    
    public List<PlayerAttackConfig> NormalAttackConfigList;
    public PlayerAttackConfig NormalCounterAttackConfig;
    public List<PlayerChargeConfig> HeavyCounterAttackConfigList;

    public List<float> ProjectileCounterAddedVelocity;

    [Header("Dodge Config")]
    public DodgeData DodgeConfig;

    [Header("Stamina Stats")]
    public float StaminaRegenPerSecond;

    [Header("Items")]
    public int CurrentPotion;
    public int MaxPotion;
    public int PotionHealAmount;
    
    [Header("Status Config")]
    public float KnockDownDuration;
    
    [Header("Movement Stats")]
    public float MoveSpeed;
    public float RotateSpeed;
    public float ChargeMoveSpeed;
    public float ChargeRotateSpeed;
    
    [Header("Position")]
    public Vector3 LastPosition;
    public Vector3 RespawnPostion;

    [Header("Abilities")]
    public List<string> AcquiredAbilityIds = new List<string>();

    public PlayerData()
    {
        Money = 0;
        
        CurrentHealth = 100;
        MaxHealth = 100;
        CurrentStamina = 100f;
        MaxStamina = 100f;
        
        AttackDamageMultiplier = 0f;
        AttackRegainRate = 0f;
        PlusNormalAttackSpeedMultiplier = 0f;
        MaxNormalAttackSpeedMultiplier = 0f;
        ParryStackDamageMultipliers = new List<float> { 1.0f, 1.1f, 1.2f, 1.3f };
        StaminaRegenPerSecond = 5f;
        CurrentSpecialAttackId = "";
        
        ChargeStamina = 0f;
        MaxChargeTime = 0f;
        CounterAngle = 0f;
        
        NormalAttackConfigList = new List<PlayerAttackConfig>();
        NormalCounterAttackConfig = new PlayerAttackConfig(); // Struct defaults
        HeavyCounterAttackConfigList = new List<PlayerChargeConfig>();
        DodgeConfig = new DodgeData(); // Class defaults

        CurrentPotion = 3;
        MaxPotion = 3;
        PotionHealAmount = 0;
        
        KnockDownDuration = 0f;
        
        MoveSpeed = 0f;
        RotateSpeed = 0f;
        ChargeMoveSpeed = 0f;
        ChargeRotateSpeed = 0f;

        LastPosition = Vector3.zero;
        RespawnPostion = Vector3.zero;

        AcquiredAbilityIds = new List<string>();
    }

    /// <summary>
    /// PlayerDataSO의 데이터로 초기화합니다.
    /// </summary>
    public void InitializeFromSO(PlayerDataSO so)
    {
        if (so == null)
        {
            return;
        }

        // Basic & Stats
        MaxHealth = so.MaxHealth;
        CurrentHealth = MaxHealth;
        MaxStamina = so.MaxStamina;
        CurrentStamina = MaxStamina;
        
        // Items
        MaxPotion = so.MaxPotion;
        CurrentPotion = MaxPotion;
        PotionHealAmount = so.PotionHealAmount;

        // Stamina
        StaminaRegenPerSecond = so.StaminaRegenPerSecond;

        // Combat Stats
        // SO에 없는 런타임 전용 스탯은 0으로 초기화
        AttackDamageMultiplier = 0f;
        AttackRegainRate = 0f;
        PlusNormalAttackSpeedMultiplier = 0f;
        MaxNormalAttackSpeedMultiplier = so.MaxNormalAttackSpeedMultiplier;
        ParryStackDamageMultipliers = new List<float>(so.ParryStackDamageMultipliers);
        CurrentSpecialAttackId = "";

        // Combat Config
        ChargeStamina = so.ChargeStamina;
        MaxChargeTime = so.MaxChargeTime;
        CounterAngle = so.CounterAngle;
        KnockDownDuration = so.KnockDownDuration;

        // Movement
        MoveSpeed = so.MoveSpeed;
        RotateSpeed = so.RotateSpeed;
        ChargeMoveSpeed = so.ChargeMoveSpeed;
        ChargeRotateSpeed = so.ChargeRotateSpeed;

        // Lists & Complex Types (Deep Copy)
        NormalAttackConfigList = new List<PlayerAttackConfig>(so.NormalAttackConfigList);
        NormalCounterAttackConfig = so.NormalCounterAttackConfig;
        HeavyCounterAttackConfigList = new List<PlayerChargeConfig>(so.HeavyCounterAttackConfigList);
        ProjectileCounterAddedVelocity = new List<float>(so.ProjectileCounterAddedVelocity);

        DodgeConfig = new DodgeData();
        if (so.DodgeConfig != null)
        {
            DodgeConfig.AnimationStateName = so.DodgeConfig.AnimationStateName;
            DodgeConfig.Type = so.DodgeConfig.Type;
            DodgeConfig.StaminaAmount = so.DodgeConfig.StaminaAmount;
            DodgeConfig.isInivicible = so.DodgeConfig.isInivicible;
            DodgeConfig.MoveConfig = so.DodgeConfig.MoveConfig;
        }
    }
}
