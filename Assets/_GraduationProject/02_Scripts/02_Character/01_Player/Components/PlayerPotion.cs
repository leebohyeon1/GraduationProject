using System;
using UnityEngine;

public class PlayerPotion : MonoBehaviour
{
    private int _maxPotion;
    private int _currentPotion;

    public event Action<int> OnPotionChange;

    public int CurrentPotion => _currentPotion;

    public void Initialize(PlayerStats stats)
    {
        _maxPotion = stats.RuntimeData.MaxPotion;
        ReloadPotion();
    }


    public void UsePotion()
    {
        if(_currentPotion == 0)
        {
            return;
        }

        _currentPotion--;
        OnPotionChange?.Invoke(_currentPotion);
    }

    public void ReloadPotion()
    {
        _currentPotion = _maxPotion;
        OnPotionChange?.Invoke(_currentPotion);
    }
}
