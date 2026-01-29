using System;
using UnityEngine;

public class PlayerPotion : MonoBehaviour
{
    private int _maxPotion;
    private int _currentPotion;

    public event Action<int> OnPotionChange;

    public int CurrentPotion => _currentPotion;

    public void Initialize(PlayerController stats)
    {
        _maxPotion = stats.Data.MaxPotion;
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
