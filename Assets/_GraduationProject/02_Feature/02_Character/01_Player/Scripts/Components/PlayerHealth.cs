using BH_Lib.Log;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;


public class PlayerHealth : MonoBehaviour, IDamageable, IHealable, IStiffness, IDisposable
{
    private PlayerStats _stats;
    private PlayerEvents _events;

    /// <summary>
    /// 경직도 관련
    /// </summary>
    private int _currentStiffness;
    private int _stiffnessThreshold = 100;
    private float _stiffnessDuration;

    public event Action<bool> OnInvisibleChanged;
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    #region Properties
    public int CurrentStiffness => _currentStiffness;
    public int StiffnessThreshold => _stiffnessThreshold;
    public float StiffnessDuration => _stiffnessDuration;

    public int Health => _stats.CurrentHealth;

    public int MaxHealth => _stats.MaxHealth;

    public bool IsDead => _stats.CurrentHealth <= 0;

    public bool IsInvincible => _stats.IsInvincible;
    #endregion

    public void Initialize(PlayerStats data, PlayerEvents evets)
    {
        _stats = data;
        _events = evets;

        _events.OnOverHeat += HandleOverHeat;
    }

    public void Dispose()
    {
        _events.OnOverHeat -= HandleOverHeat;
    }

    public void ChangeHealth(int amount)
    {
        int previousHealth = Health;
        _stats.CurrentHealth = Mathf.Clamp(_stats.CurrentHealth + amount, 0, MaxHealth);

        OnHealthChanged?.Invoke(previousHealth, Health);
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsDead || IsInvincible)
        {
            return;
        }

        if (_stats.IsDefending)
        {
            damageAmount = Mathf.RoundToInt(damageAmount *
                _stats.CombatData.DefendDamageReductionRate);

        }

        ChangeHealth(-damageAmount);

        if (IsDead)
        {
            Die();
        }
    }

    public void TakeDamage(int damageAmount, int stiffenessAmount)
    {
        // 죽었거나 무적이면 리턴
        if (IsDead || IsInvincible)
        {
            return;
        }

        // 방어중일 때 수치 경감
        if (_stats.IsDefending)
        {
            damageAmount = Mathf.RoundToInt(damageAmount *
                _stats.CombatData.DefendDamageReductionRate);

            stiffenessAmount = Mathf.RoundToInt(stiffenessAmount * 0.5f);
        }

        ChangeHealth(-damageAmount);
        AddStiffness(stiffenessAmount);

        if (IsDead)
        {
            Die();
        }
    }

    public void AddStiffness(int amount)
    {
        ChangeStiffness(amount);

        // 현재 경직도가 최대 경직도를 넘을 때
        if(_currentStiffness >= _stiffnessThreshold)
        {
            // 경직도 초기화
            ChangeStiffness(-_currentStiffness);
            // 강한 경직
            HeavyStagger();
        }
        else
        {
            // 약한 경직
            LightStagger();
        }
    }

    /// <summary>
    /// 경직도 변경 함수
    /// </summary>
    /// <param name="amount">경직도 변경량</param>
    private void ChangeStiffness(int amount)
    {
        _currentStiffness += amount;
    }

    /// <summary>
    /// 약한 경직
    /// </summary>
    private void LightStagger()
    {
        _stiffnessDuration = _stats.CombatData.LightStaggerDuration;
        _stats.SetDamagedType(PlayerDamagedType.Normal);
    }

    /// <summary>
    /// 강한 경직
    /// </summary>
    private void HeavyStagger()
    {
        _stiffnessDuration = _stats.CombatData.HeavyStaggerDuration;
        _stats.SetDamagedType(PlayerDamagedType.Strong);
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
        _stats.IsInvincible = isInvisible;
    }

    private void HandleOverHeat(int damage)
    {
        if(_stats.IsOverHeat)
        {
            TakeDamage(damage);
        }
    }
}
