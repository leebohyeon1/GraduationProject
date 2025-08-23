using System;
using BH_Lib.DI;
using UnityEngine;

/// <summary>
/// 모든 캐릭터의 기본 클래스
/// IDamageable 인터페이스를 구현하여 체력과 피해 시스템을 제공
/// </summary>
public class CharacterBase : DIMonoBehaviour, IDamageable
{
    [Header("Character Settings")]
    [SerializeField] protected CharacterStats _stats;
    
    [Header("Runtime Stats")]
    [SerializeField] protected float _currentHealth;
    
    public float Health => _currentHealth;
    public float MaxHealth => _stats.MaxHealth;
    public bool IsDead => _currentHealth <= 0;
    
    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;
    
    protected override void Awake()
    {
        base.Awake();

        if (_stats != null)
        {
            _currentHealth = _stats.MaxHealth;
        }
    }
    
    public virtual void TakeDamage(float damageAmount, GameObject damageSource)
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
    }
    
    public virtual void Heal(float healAmount)
    {
        if (IsDead) return;
        
        float previousHealth = _currentHealth;
        _currentHealth = Mathf.Min(_stats.MaxHealth, _currentHealth + healAmount);
        
        OnHealthChanged?.Invoke(previousHealth, _currentHealth);
    }
}
