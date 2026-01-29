using System;
using UnityEngine;

public class PlayerPotion : MonoBehaviour
{
    private PlayerEvents _events;

    private int _maxPotion;
    private int _currentPotion;

    private int _potionHealAmount;

    public event Action<int> OnPotionChange;

    public int MaxPotion => _maxPotion;
    public int CurrentPotion => _currentPotion;
    public int PotionHealAmount => _potionHealAmount;   

    public void Initialize(PlayerController player)
    {
        _events = player.Events;

        _maxPotion = player.Data.MaxPotion;
        ReloadPotion();

        _potionHealAmount = player.Data.PotionHealAmount;
    }

    /// <summary>
    /// 포션 사용 함수
    /// </summary>
    public void UsePotion()
    {
        if(_currentPotion == 0)
        {
            return;
        }

        _currentPotion--;
        OnPotionChange?.Invoke(_currentPotion);
        _events.TriggerHeal(_potionHealAmount);
    }

    /// <summary>
    /// 포션 재장전 함수
    /// </summary>
    public void ReloadPotion()
    {
        _currentPotion = _maxPotion;
        OnPotionChange?.Invoke(_currentPotion);
    }
}
