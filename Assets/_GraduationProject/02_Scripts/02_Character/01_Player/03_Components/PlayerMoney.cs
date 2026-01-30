using System;
using UnityEngine;

/// <summary>
/// 플레이어 돈 시스템
/// </summary>
public class PlayerMoney : MonoBehaviour
{
    private PlayerEvents _events;

    private int _currentMoney;
    public int CurrentMoney => _currentMoney;

    public event Action<int> MoneyChanged;   

    public void Initialize(PlayerController player)
    {
        _events = player.Events;

        if (player.RuntimeData != null)
        {
            _currentMoney = player.RuntimeData.gold;
        }
        else
        {
            _currentMoney = 0;
        }
    }

    /// <summary>
    /// 돈을 사용할 수 있는지 확인
    /// </summary>
    /// <param name="amount">사용량</param>
    /// <returns>사용 가능 여부</returns>
    public bool CanUseMoney(int amount)
    {
        if (_currentMoney >= amount)
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
        _currentMoney -= amount;
        MoneyChanged?.Invoke(amount);
    }

    /// <summary>
    /// 돈 획득 함수
    /// </summary>
    /// <param name="amount">획득량</param>
    public void GetMoney(int amount)
    {
        _currentMoney += amount;
        MoneyChanged?.Invoke(amount);
    }
}
