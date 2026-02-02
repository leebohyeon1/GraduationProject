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

    public event Action<int> MoneyChanged;   

    public void Initialize(PlayerController player)
    {
        _events = player.Events;
        _data = player.RuntimeData;

        StartCoroutine(Asd());
    }

    /// <summary>
    /// 돈을 사용할 수 있는지 확인
    /// </summary>
    /// <param name="amount">사용량</param>
    /// <returns>사용 가능 여부</returns>
    public bool CanUseMoney(int amount)
    {
        if (_data == null) return false;

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
        if (_data == null) return;

        _data.Money -= amount;
        MoneyChanged?.Invoke(_data.Money);
    }

    /// <summary>
    /// 돈 획득 함수
    /// </summary>
    /// <param name="amount">획득량</param>
    public void GetMoney(int amount)
    {
        if (_data == null) return;

        _data.Money += amount;
        MoneyChanged?.Invoke(_data.Money);
    }

    private IEnumerator Asd()
    {
        yield return new WaitForSeconds(2f);

        GetMoney(10);

        yield return new WaitForSeconds(1f);
        
        GetMoney(20);

        yield return new WaitForSeconds(10f);

        GetMoney(100);
    }
}
