using System;
using System.ComponentModel;
using UnityEngine;

public class HealthSystem : MonoBehaviour, IHealth
{
    #region Protected Fields
    protected int p_health;
    protected int p_maxHealth;
    protected bool p_isInvincible;
    protected bool p_isHit;
    #endregion

    #region Properties
    public int Health => p_health;

    public int MaxHealth => p_maxHealth;

    public bool IsDead => p_health <= 0;

    public bool IsInvincible => p_isInvincible;

    public bool IsHit => p_isHit;
    #endregion

    #region Events
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;
    public event Action<bool> OnInvisibleChanged;
    #endregion


    public void ChangeHealth(int amount)
    {
        int previousHealth = Health;
        p_health = Mathf.Clamp(p_health + amount, 0, MaxHealth);

        OnHealthChanged?.Invoke(previousHealth , Health);
    }

    public virtual void TakeDamage(int damageAmount, IAttacker attacker = null)
    {
        if (IsDead || IsInvincible)
        {
                return;
        }

        ChangeHealth(-damageAmount);

        if (IsDead)
        {
            Die();
        }
        else
        {
            p_isHit = true;
        }

    }

    public virtual void TakeDamage(int damageAmount, int StiffenessAmount, IAttacker attacker = null)
    {

    }

    public void Heal(int healAmount)
    {
        if (IsDead)
        {
            return;
        }
            
        ChangeHealth(healAmount);
    }

    public void Die()
    {
        OnDied?.Invoke();
    }

    public void SetInvisible(bool isInvisible)
    {
        p_isInvincible = isInvisible;

        OnInvisibleChanged?.Invoke(isInvisible);
    }

    public void ResetHitState()
    {
        p_isHit = false;
    }

}

