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
    private PlayerDataSO _baseData;
    private PlayerDataSO _data;
    private PlayerDataSO _runtimeData;

    // State
    public bool IsInvincible; // 무적인가?
    public bool IsParring; // 패리중인가?
    public bool IsLockOn; // 락온중인가?

    public bool IsMiddleHit; // 약한 피격중인가?
    public bool IsHeavyHit;  // 강한 피격중인가?
    public bool IsKnockDown;
    public bool IsDamaged => IsMiddleHit || IsHeavyHit || IsKnockDown; // 피격중인가?

    public bool CanRegenStamina;

    public float LastTargetChangeTime;
    public const float TARGET_CHANGE_COOLDOWN = 0.25f; // 0.25초 딜레이

    // Stat
    public int CurrentHealth; // 현재 체력
    public float CurrentStamina; // 현재 열기

    // Combat
    public HashSet<IParryable> ParrySet = new HashSet<IParryable>();
    public int ChargeLevel = 0;
    public int AttackComboIndex = 0;

    // Properties
    public PlayerDataSO Data => _data;  
    public PlayerDataSO RuntimeData => _runtimeData;

    public List<PlayerAttackData> AttackDatas => _runtimeData.CombatData.AttackDatas;

    public PlayerAttackData CurrentAttackData => AttackDatas[AttackComboIndex];
    public PlayerChargeConfig CurrentChargeAttackData => AttackDatas[AttackComboIndex].ChargeConfigs[ChargeLevel];


    public PlayerStats(PlayerDataSO baseData, PlayerEvents events)
    {
        _baseData = baseData;
        _data = UnityEngine.Object.Instantiate(baseData);
        _runtimeData = UnityEngine.Object.Instantiate(_data);

        _events = events;

        CurrentHealth = _data.MaxHealth;
        CurrentStamina = _data.MaxStamina;

        _events.ParryWindowFinished += OnParryWindowFinished;
        _events.AttackFinished += OnAttackFinished;
    }

    public void Dispose()
    {
        _events.ParryWindowFinished -= OnParryWindowFinished;
        _events.AttackFinished -= OnAttackFinished;
    }

    /// <summary>
    /// 패링 검사가 종료되는 시점
    /// </summary>
    private void OnParryWindowFinished()
    {
        if (ParrySet.Count > 0)
        {
            ParrySet.Clear();
        }
    }

    public void StatUpgrade(PlusPlayerStat stat)
    {
        _runtimeData.MaxHealth += stat.Health;
        _runtimeData.MaxStamina += stat.Stamina;
        _runtimeData.StaminaRegenPerSecond += stat.StaminaRegenPerSecond;

        for (int i = 0; i < stat.CombatData.AttackDatas.Count; i++)
        {
            var baseAttackData = _runtimeData.CombatData.AttackDatas[i];
            var plusAttackData = stat.CombatData.AttackDatas[i];
            baseAttackData.AttackConfig.AttackDamage += plusAttackData.AttackConfig.AttackDamage;
            baseAttackData.AttackConfig.AttackStamina += plusAttackData.AttackConfig.AttackStamina;
            baseAttackData.AttackConfig.StiffnessAmount += plusAttackData.AttackConfig.StiffnessAmount;
            baseAttackData.AttackConfig.AttackRadius += plusAttackData.AttackConfig.AttackRadius;
            baseAttackData.AttackConfig.KnockBackDuration += plusAttackData.AttackConfig.KnockBackDuration;
            baseAttackData.AttackConfig.KnockBackForce += plusAttackData.AttackConfig.KnockBackForce;

            for(int j = 0; j < stat.CombatData.AttackDatas[i].ChargeConfigs.Count; j++)
            {
                var baseChargeConfig = baseAttackData.ChargeConfigs[j];
                var plusChargeConfig = plusAttackData.ChargeConfigs[j];
                baseChargeConfig.AttackConfig.AttackDamage += plusChargeConfig.AttackConfig.AttackDamage;
                baseChargeConfig.AttackConfig.AttackStamina += plusChargeConfig.AttackConfig.AttackStamina;
                baseChargeConfig.AttackConfig.StiffnessAmount += plusChargeConfig.AttackConfig.StiffnessAmount;
                baseChargeConfig.AttackConfig.AttackRadius += plusChargeConfig.AttackConfig.AttackRadius;
                baseChargeConfig.AttackConfig.KnockBackDuration += plusChargeConfig.AttackConfig.KnockBackDuration;
                baseChargeConfig.AttackConfig.KnockBackForce += plusChargeConfig.AttackConfig.KnockBackForce;
            }

            for(int j = _runtimeData.CombatData.AttackDatas[i].ChargeConfigs.Count; j < plusAttackData.ChargeConfigs.Count; j++)
            {
                baseAttackData.ChargeConfigs.Add(plusAttackData.ChargeConfigs[j]);
            }
        }

        _runtimeData.CombatData.ChargeStamina += stat.CombatData.ChargeStamina;

    }

    private void OnAttackFinished()
    {
        int nextIndex = AttackComboIndex + 1;
        int maxAttackIndex = AttackDatas.Count - 1;

        if (nextIndex >= maxAttackIndex)
        {
            AttackComboIndex = maxAttackIndex;
            return;
        }

        AttackComboIndex = nextIndex;
    }

}