using System;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 체력 시스템을 담당하는 클래스
/// IDamageable과 IHealable 인터페이스를 구현하여 피해와 회복을 처리합니다.
/// </summary>
public class PlayerHealth : MonoBehaviour, IPlayerHealth
{
    [Header("Runtime Stats")]
    [SerializeField] protected int p_currentHealth;

    /// <summary>
    /// 피격 상태 플래그 (상태 머신에서 Hit 상태 전환용)
    /// </summary>
    private bool _isHit = false;

    /// <summary>
    /// 방어 상태 플래그 (방어 중일 때 데미지 감소)
    /// </summary>
    private bool _isDefending = false;

    /// <summary>
    /// 무적 상태 여부
    /// </summary>
    private bool _isInvincible = false;

    /// <summary>
    /// 플레이어 컨텍스트 참조
    /// </summary>
    private PlayerContext _context;

    /// <summary>
    /// 현재 체력
    /// </summary>
    public int Health => p_currentHealth;

    /// <summary>
    /// 최대 체력
    /// </summary>
    public int MaxHealth => _context.Stats.MaxHealth;

    /// <summary>
    /// 사망 여부
    /// </summary>
    public bool IsDead => p_currentHealth <= 0;

    /// <summary>
    /// 생존 여부
    /// </summary>
    public bool IsAlive => !IsDead;

    /// <summary>
    /// 피격 상태 여부 (Hit 상태 전환 조건)
    /// </summary>
    public bool IsHit => _isHit;


    /// <summary>
    /// 체력 변경 이벤트
    /// </summary>
    public event Action<int, int> OnHealthChanged;

    /// <summary>
    /// 사망 이벤트
    /// </summary>
    public event Action OnDeath;

    /// <summary>
    /// Hit 상태 플래그를 리셋합니다
    /// PlayerHitState에서 상태 종료 시 호출
    /// </summary>
    public void ResetHitState()
    {
        _isHit = false;
    }

    /// <summary>
    /// 방어 상태를 설정합니다
    /// PlayerDefendState에서 호출
    /// </summary>
    /// <param name="isDefending">방어 상태 여부</param>
    public void SetDefending(bool isDefending)
    {
        _isDefending = isDefending;
    }

    /// <summary>
    /// 플레이어 체력 시스템 초기화
    /// </summary>
    /// <param name="context">플레이어 컨텍스트</param>
    public void Initialize(PlayerContext context)
    {
        _context = context;

        // 최대 체력으로 초기화
        if (_context.Stats != null)
        {
            p_currentHealth = _context.Stats.MaxHealth;
        }

        // 이벤트 버스 구독
        OnHealthChanged += (previousHealth, currentHealth) => _context.EventBus.PublishHealthChanged(previousHealth, currentHealth);
        OnDeath += _context.EventBus.PublishPlayerDied;
        _context.EventBus.OnDodgeStart += ()=> { SetInvisible(true); };
        _context.EventBus.OnDodgeEnd += ()=> { SetInvisible(false); };

    }

    /// <summary>
    /// 피해를 입는 처리
    /// </summary>
    /// <param name="damageAmount">피해량</param>
    /// <param name="attacker">공격자</param>
    public void TakeDamage(int damageAmount, IAttacker attacker)
    {
        if (IsDead || _isInvincible) return;

        // 방어 중이면 데미지 30%만 받음 (70% 감소)
        if (_isDefending)
        {
            damageAmount = Mathf.RoundToInt(damageAmount * _context.Stats.DefendDamageReductionRate);
        }

        int previousHealth = p_currentHealth;
        p_currentHealth = Mathf.Max(0, p_currentHealth - damageAmount);

        // 체력 변경 이벤트 발행
        OnHealthChanged?.Invoke(previousHealth, p_currentHealth);

        if (IsDead)
        {
            Die();
        }
        else
        {
            // 피격 상태 설정 (Hit 상태로 전환하기 위한 플래그)
            _isHit = true;
        }
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    protected virtual void Die()
    {
        // 사망 이벤트 발행
        _context.EventBus.PublishPlayerDied();

        Log.Print("플레이어 사망!");

        // TODO: 각 시스템 비활성화 처리
    }

    /// <summary>
    /// 체력 회복 처리
    /// </summary>
    /// <param name="healAmount">회복량</param>
    public virtual void Heal(int healAmount)
    {
        if (IsDead) return;

        int previousHealth = p_currentHealth;
        p_currentHealth = Mathf.Min(_context.Stats.MaxHealth, p_currentHealth + healAmount);

        // 체력 변경 이벤트 발생
        OnHealthChanged?.Invoke(previousHealth, p_currentHealth);
    }

    private void SetInvisible(bool isInvisible)
    {
        _isInvincible = isInvisible;

        Log.PrintColor(Color.yellow, $"무적: {isInvisible}");
    }

    private void OnDestroy()
    {
        OnHealthChanged -= _context.EventBus.PublishHealthChanged;
        OnDeath -= _context.EventBus.PublishPlayerDied;
        _context.EventBus.OnDodgeStart -= ()=> { SetInvisible(true); };
        _context.EventBus.OnDodgeEnd -= ()=> { SetInvisible(false); };
    }
}
