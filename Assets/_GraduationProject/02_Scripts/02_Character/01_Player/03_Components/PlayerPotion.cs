using System;
using UnityEngine;

public class PlayerPotion : MonoBehaviour
{
    private PlayerEvents _events;
    private PlayerData _data;

    public event Action<int> OnPotionChange;

    public int MaxPotion => _data != null ? _data.MaxPotion : 3;
    public int CurrentPotion => _data != null ? _data.CurrentPotion : 0;
    public int PotionHealAmount => _data != null ? _data.PotionHealAmount : 40;

    public void Initialize(PlayerController player)
    {
        _events = player.Events;
        _data = player.RuntimeData;

        // _potionHealAmount = player.Data.PotionHealAmount;
        if (_data != null && _data.PotionHealAmount == 0)
        {
            _data.PotionHealAmount = player.Data.PotionHealAmount;
        }

        // UI 업데이트를 위해 이벤트 호출
        if (_data != null)
        {
            OnPotionChange?.Invoke(_data.CurrentPotion);
        }
    }

    /// <summary>
    /// 포션 사용 함수
    /// </summary>
    public void UsePotion()
    {
        if (_data == null || _data.CurrentPotion <= 0)
        {
            return;
        }

        _data.CurrentPotion--;
        OnPotionChange?.Invoke(_data.CurrentPotion);
        _events.TriggerHeal(PotionHealAmount);
    }

    /// <summary>
    /// 포션 재장전 함수
    /// </summary>
    public void ReloadPotion()
    {
        if (_data == null)
        {
            return;
        }

        _data.CurrentPotion = _data.MaxPotion;
        OnPotionChange?.Invoke(_data.CurrentPotion);
    }

    /// <summary>
    /// 포션 사용횟수 증가 함수
    /// </summary>
    /// <param name="amount">증가량</param>
    public void IncreaseMaxPotion(int amount)
    {
        if (_data == null)
        {
            return;
        }

        _data.MaxPotion += amount;
    }
}
