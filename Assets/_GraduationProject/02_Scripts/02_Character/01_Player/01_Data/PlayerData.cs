using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    [Header("Basic Info")]
    public int Money;
    
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
    public string CurrentSpecialAttackId;
    
    [Header("Combat Config")]
    public float ChargeStamina;
    public float MaxChargeTime;
    public float CounterAngle;
    
    public List<PlayerAttackConfig> NormalAttackConfigList;
    public PlayerAttackConfig NormalCounterAttackConfig;
    public List<PlayerChargeConfig> HeavyCounterAttackConfigList;

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
    public float x, y, z;

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
        
        x = 0;
        y = 0;
        z = 0;
        
        AcquiredAbilityIds = new List<string>();
    }
}
