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
    private PlayerDataSO _data;

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

    // Properties
    public PlayerDataSO Data => _data;

    public List<PlayerAttackData> AttackDatas => _data.CombatData.AttackDatas;

    public PlayerStats(PlayerDataSO baseData, PlayerEvents events)
    {
        _data = baseData;
        _events = events;

        CurrentHealth = _data.MaxHealth;
        CurrentStamina = _data.MaxStamina;

        _events.ParryWindowFinished += OnParryWindowFinished;
    }

    public void Dispose()
    {
        _events.ParryWindowFinished -= OnParryWindowFinished;
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

    public void StatUpgrade(PlusPlayerStat  stat)
    {
        _data.MaxHealth += stat.Health;
        _data.MaxStamina += stat.Stamina;
        _data.StaminaRegenPerSecond += stat.StaminaRegenPerSecond;

        for(int i =0; i < stat.CombatData.AttackDatas.Count; i++)
        {
            var baseAttackData = _data.CombatData.AttackDatas[i];
            var plusAttackData = stat.CombatData.AttackDatas[i];
            baseAttackData.AttackDamage += plusAttackData.AttackDamage;
            baseAttackData.AttackStamina += plusAttackData.AttackStamina;
            baseAttackData.StiffnessAmount += plusAttackData.StiffnessAmount;
            baseAttackData.AttackRadius += plusAttackData.AttackRadius;
            baseAttackData.KnockBackDuration += plusAttackData.KnockBackDuration;
            baseAttackData.KnockBackForce += plusAttackData.KnockBackForce;
        }

        _data.CombatData.ChargeStamina += stat.CombatData.ChargeStamina;

        for (int i =0; i < stat.CombatData.ChargeAttackDatas.Count; i++)
        {
            var baseChargeAttackData = _data.CombatData.ChargeAttackDatas[i];
            var plusChargeAttackData = stat.CombatData.ChargeAttackDatas[i];
            baseChargeAttackData.AttackData.AttackDamage += plusChargeAttackData.AttackData.AttackDamage;
            baseChargeAttackData.AttackData.AttackStamina += plusChargeAttackData.AttackData.AttackStamina;
            baseChargeAttackData.AttackData.StiffnessAmount += plusChargeAttackData.AttackData.StiffnessAmount;
            baseChargeAttackData.AttackData.AttackRadius += plusChargeAttackData.AttackData.AttackRadius;
            baseChargeAttackData.AttackData.KnockBackDuration += plusChargeAttackData.AttackData.KnockBackDuration;
            baseChargeAttackData.AttackData.KnockBackForce += plusChargeAttackData.AttackData.KnockBackForce;
        }

        for(int i = stat.CombatData.ChargeAttackDatas.Count; i < stat.CombatData.ChargeAttackDatas.Count; i++)
        {
            _data.CombatData.ChargeAttackDatas.Add(stat.CombatData.ChargeAttackDatas[i]);
        }



    }
}