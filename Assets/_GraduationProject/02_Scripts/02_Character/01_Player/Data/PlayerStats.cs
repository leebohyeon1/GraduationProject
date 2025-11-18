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
    public bool IsDefending; // 방어중인가?
    public bool IsInvincible; // 무적인가?
    public bool IsParring; // 패리중인가?
    public bool IsLockOn; // 락온중인가?

    public bool IsMiddleHit; // 약한 피격중인가?
    public bool IsHeavyHit;  // 강한 피격중인가?
    public bool IsKnockDown;
    public bool IsDamaged => IsMiddleHit || IsHeavyHit || IsKnockDown; // 피격중인가?

    public bool CanRegenStamina;

    // Stat
    public int CurrentHealth; // 현재 체력
    public float CurrentStamina; // 현재 열기

    // Combat
    public HashSet<IParryable> ParrySet = new HashSet<IParryable>();
    public int ChargeLevel = 0;

    // Properties
    public PlayerDataSO Data => _data;

    public PlayerAttackDataSO[] AttackDatas => _data.CombatData.AttackDatas;

    public PlayerStats(PlayerDataSO baseData, PlayerEvents events)
    {
        _data = baseData;
        _events = events;

        CurrentHealth = _data.MaxHealth;
        CurrentStamina = _data.MaxStamina;
    }

    public void Dispose()
    {
    }


}