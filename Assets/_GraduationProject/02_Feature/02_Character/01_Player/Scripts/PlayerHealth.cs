using System;
using BH_Lib.Log;
using UnityEngine;

public class PlayerHealth : PlayerComponent, IDamageable, IHealable
{
    [Header("Runtime Stats")]
    [SerializeField] protected float _currentHealth;

    public float Health => _currentHealth;
    public float MaxHealth => p_playerStats.MaxHealth;
    public bool IsDead => _currentHealth <= 0;
    public bool IsAlive => !IsDead;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    public override void Initialize(Player player)
    {
        base.Initialize(player);
        
        if (p_playerStats != null)
        {
            _currentHealth = p_playerStats.MaxHealth;
        }
    }

    public void TakeDamage(float damageAmount, GameObject damageSource)
    {
        if (IsDead) return;

        float previousHealth = _currentHealth;
        _currentHealth = Mathf.Max(0, _currentHealth - damageAmount);

        OnHealthChanged?.Invoke(previousHealth, _currentHealth);

        if (IsDead)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke();

        Log.Print("플레이어 사망!");

        // 각 시스템 비활성화
        if (p_player.PlayerController != null)
        {
            p_player.PlayerController.enabled = false;
        }

        if (p_player.PlayerMovement != null)
        {
            p_player.PlayerMovement.enabled = false;
        }

        if (p_player.PlayerAttack != null)
        {
            p_player.PlayerAttack.SetAttackEnabled(false);
        }
    }

    public virtual void Heal(float healAmount)
    {
        if (IsDead) return;

        float previousHealth = _currentHealth;
        _currentHealth = Mathf.Min(p_playerStats.MaxHealth, _currentHealth + healAmount);

        OnHealthChanged?.Invoke(previousHealth, _currentHealth);
    }
}
