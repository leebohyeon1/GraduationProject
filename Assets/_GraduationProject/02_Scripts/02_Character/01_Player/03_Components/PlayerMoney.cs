using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어 돈 시스템
/// </summary>
public class PlayerMoney : MonoBehaviour
{
    private PlayerEvents _events;
    private PlayerData _data;

    public int CurrentMoney => _data != null ? _data.Money : 0;
    public int CurrentSpecialMoney => _data != null ? _data.SpecialMoney : 0;   

    public event Action<int> MoneyChanged;   
    public event Action<int> SpecialMoneyChanged;   

    public void Initialize(PlayerController player)
    {
        _events = player.Events;
        _data = player.RuntimeData;
    }

    /// <summary>
    /// 돈을 사용할 수 있는지 확인
    /// </summary>
    /// <param name="amount">사용량</param>
    /// <returns>사용 가능 여부</returns>
    public bool CanUseMoney(int amount)
    {
        if (_data.Money >= amount)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 돈 사용 함수
    /// </summary>
    /// <param name="amount">사용량</param>
    public void UseMoney(int amount)
    {
        _data.Money -= amount;
        MoneyChanged?.Invoke(_data.Money);
    }

    /// <summary>
    /// 돈 획득 함수
    /// </summary>
    /// <param name="amount">획득량</param>
    public void GiveMoney(int amount)
    {
        _data.Money += amount;
        MoneyChanged?.Invoke(_data.Money);
    }


    /// <summary>
    /// 돈을 사용할 수 있는지 확인
    /// </summary>
    /// <param name="amount">사용량</param>
    /// <returns>사용 가능 여부</returns>
    public bool CanUseSpecialMoney(int amount)
    {
        if (_data.SpecialMoney >= amount)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 돈 사용 함수
    /// </summary>
    /// <param name="amount">사용량</param>
    public void UseSpecialMoney(int amount)
    {
        _data.SpecialMoney -= amount;
        SpecialMoneyChanged?.Invoke(_data.SpecialMoney);
    }

    /// <summary>
    /// 돈 획득 함수
    /// </summary>
    /// <param name="amount">획득량</param>
    public void GiveSpecialMoney(int amount)
    {
        _data.SpecialMoney += amount;
        SpecialMoneyChanged?.Invoke(_data.SpecialMoney);
    }
}
