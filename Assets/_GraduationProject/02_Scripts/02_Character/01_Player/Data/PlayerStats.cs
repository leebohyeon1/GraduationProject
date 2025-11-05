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

    public bool IsLightHit; // 약한 피격중인가?
    public bool IsHeavyHit; // 강한 피격중인가?
    public bool IsDamaged => IsLightHit || IsHeavyHit; // 피격중인가?

    // Stat
    public int CurrentHealth; // 현재 체력
    public int CurrentHeat; // 현재 열기

    // Properties
    public BasePlayerDatasSO Data => _dataBase.BaseData;

    public PlayerAttackDataSO[] AttackDatas => _dataBase.BaseData.CombatData.AttackDatas;

    public PlayerStats(PlayerDataBaseSO baseData, PlayerEvents events)
    {
        _dataBase = baseData;
        _events = events;
    }

    public void Dispose()
    {
    }
}