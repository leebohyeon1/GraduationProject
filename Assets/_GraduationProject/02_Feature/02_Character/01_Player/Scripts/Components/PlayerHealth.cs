using System;
using BH_Lib.Log;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IPlayerHealth
{
    [Header("Runtime Stats")]
    [SerializeField] protected int _currentHealth;
    private bool _isHit = false;

    private PlayerContext _context;

    public int Health => _currentHealth;
    public int MaxHealth => _context.Stats.MaxHealth;
    public bool IsDead => _currentHealth <= 0;
    public bool IsAlive => !IsDead;
    public bool IsHit => _isHit;


    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    /// <summary>
    /// Hit 상태 플래그를 리셋합니다
    /// PlayerHitState에서 상태 종료 시 호출
    /// </summary>
    public void ResetHitState()
    {
        _isHit = false;
    }

    public void Initialize(PlayerContext context)
    {
        _context = context;

        if (_context.Stats != null)
        {
            _currentHealth = _context.Stats.MaxHealth;
        }
        
        _context.EventBus.OnHealthChanged += OnHealthChanged;
        _context.EventBus.OnPlayerDied += OnDeath;
    }

    public void TakeDamage(int damageAmount, IAttacker attacker)
    {
        if (IsDead) return;

        int previousHealth = _currentHealth;
        _currentHealth = Mathf.Max(0, _currentHealth - damageAmount);

        _context.EventBus.PublishHealthChanged(previousHealth, _currentHealth);

        if (IsDead)
        {
            Die();
        }
        else
        {
            // 피격 상태 설정
            _isHit = true;
        }
    }

    protected virtual void Die()
    {   
        _context.EventBus.PublishPlayerDied();

        Log.Print("플레이어 사망!");

        // 각 시스템 비활성화
    }

    public virtual void Heal(int healAmount)
    {
        if (IsDead) return;

        int previousHealth = _currentHealth;
        _currentHealth = Mathf.Min(_context.Stats.MaxHealth, _currentHealth + healAmount);

        OnHealthChanged?.Invoke(previousHealth, _currentHealth);
    }
}
