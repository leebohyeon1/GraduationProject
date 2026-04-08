using System;
using UnityEngine;

public class PlayerPotion : MonoBehaviour, IDisposable
{
    private PlayerEvents _events;
    private PlayerData _data;
    private InputReaderSO _inputReader;

    public event Action<int> OnPotionChange;

    public int MaxPotion => _data != null ? (int)_data.Potion.Value : 3;
    public int CurrentPotion => _data != null ? _data.CurrentPotion : 0;
    public int PotionHealAmount => _data != null ? (int)_data.PotionHealAmount.Value : 40;

    public void Initialize(PlayerController player)
    {
        _events = player.Events;
        _data = player.RuntimeData;
        _inputReader = player.InputReader;

        _inputReader.PotionEvent += OnPotionEvent;

        // UI 업데이트를 위해 이벤트 호출
        if (_data != null)
        {
            OnPotionChange?.Invoke(_data.CurrentPotion);
        }

        // 리소스 해제 등록
        player.RegisterDisposable(this);
    }

    public void Dispose()
    {
        _inputReader.PotionEvent -= OnPotionEvent;

        OnPotionChange = null;
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

        Debug.Log("포션 사용");

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

        _data.CurrentPotion = (int)_data.Potion.Value;
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

        _data.Potion.AddModifier(new StatModifier(amount, StatModifierType.Flat, this));
    }

    private void OnPotionEvent()
    {
        UsePotion();
    }

}
