using System;
using UnityEngine;
using UnityEngine.Events;

public class HitTower : MonoBehaviour, IDamageable
{
    private int _currentHealth;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => 1;
    public bool IsDead => CurrentHealth <= 0;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    public UnityEvent OnTowerHit;   
    public UnityEvent OnTowerReset;   

    public void ResetTower()
    {
        int previousHealth = _currentHealth;    
        _currentHealth = MaxHealth;
        OnHealthChanged?.Invoke(previousHealth, _currentHealth);
    }

    public void TakeDamage(DamageData damageData)
    {
        if(IsDead)
        {
            return;
        }

        // 플레이어가 아니면 리턴
        if(!damageData.AttackerTransform.TryGetComponent<PlayerController>(out PlayerController component))
        {
            return;
        }

        if (damageData.AttackType >= AttackType.Strong_1)
        {
            Debug.Log("타워 때리기 퍼즐 상호작용!");

            int previousHealth = _currentHealth;
            _currentHealth = Mathf.Max(_currentHealth - damageData.DamageAmount, 0);
            OnHealthChanged?.Invoke(previousHealth, _currentHealth);
            
            if (IsDead)
            {
                OnTowerHit?.Invoke();
                OnDied?.Invoke();
            }
        }
    }
}
