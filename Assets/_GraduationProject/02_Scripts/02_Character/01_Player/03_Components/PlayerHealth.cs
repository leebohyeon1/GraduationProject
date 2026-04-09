using System;
using UnityEngine;

/// <summary>
/// 플레이어 데미지 문맥
/// </summary>
public struct PlayerDamageContext
{
    public DamageData Data;     // 데미지 데이터
    public bool HasSuperArmor;  // 슈퍼아머 여부
}

/// <summary>
/// 플레이어의 체력, 데미지 처리, 사망, 무적 상태 등을 관리하는 컴포넌트입니다.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable, IHealable, IStiffness, IDisposable
{    
    private PlayerEvents _events; // 플레이어 이벤트
    private PlayerData _data;     // 런타임 데이터 (직접 참조)

    public event Action<int, int> OnHealthChanged; // 체력 변경 이벤트 (Previous, Current)
    public event Action OnDied; // 사망 이벤트
    public event Action<int> TakeDamged;

    [Header("Stiffness")]
    [SerializeField] private int _currentStiffness; // 현재 경직도
    private float _stiffnessDuration; // 경직 지속 시간

    public event Action<int, int> OnStiffnessChanged;

    [Header("Properties")]
    public int CurrentHealth => _data != null ? _data.CurrentHealth : 0;
    public int MaxHealth => _data != null ? (int)_data.Health.Value : 100;
    public bool IsDead => CurrentHealth <= 0; // 사망 여부

    public int CurrentStiffness => _currentStiffness; // 현재 경직도
    public int StiffnessThreshold => 100; // 경직 임계값
    public float StiffnessDuration => _stiffnessDuration; // 경직 지속 시간
    
    public float KnockDownDuration => _data != null ? _data.KnockDownDuration.Value : 3f;

    public bool invincibility { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


    /// <summary>
    /// 초기화 함수
    /// </summary>
    /// <param name="player">플레이어 컨트롤러 (초기화용)</param>
    public void Initialize(PlayerController player)
    {
        _data = player.RuntimeData;
        _events = player.Events;

        // 경직도 초기화
        _currentStiffness = 0;

        // 이벤트 해제 구독
        _events.AttackRegained += OnAttackRegained;
        _events.Heal += OnHeal;

        // 리소스 해제 등록
        player.RegisterDisposable(this);
    }

    /// <summary>
    /// 리소스 해제 함수
    /// </summary>
    public void Dispose()
    {
        _events.AttackRegained -= OnAttackRegained;

        OnHealthChanged = null;
        OnDied = null; 
        TakeDamged = null;
    }

    public void TakeDamage(DamageData damageData)
    {
        if (IsDead)
        {
            return;
        }

        PlayerDamageContext damageContext = new PlayerDamageContext()
        {
            Data = damageData,
            HasSuperArmor = false
        };

        // 데미지 계산 전 레퍼런스로 데미지 데이터 수정
        _events.TriggerBeforeDamaged(ref damageContext);

        damageData = damageContext.Data;

        TakeDamged?.Invoke(damageData.DamageAmount);
        ChangeHealth(-damageData.DamageAmount);

        if (IsDead)
        {
            Die();
            return;
        }

        AddStiffness(damageData.StiffnessAmount);

        // 경직도 임계값을 넘으면
        if (CurrentStiffness >= StiffnessThreshold)
        {
            _events.TriggerKnockdown();
            ChangeStiffness(-_currentStiffness); // 경직도 초기화
            return;
        }

        if (!damageContext.HasSuperArmor)   // 슈퍼 아머가 아니면
        {
            _events.TriggerDamaged(damageData);
            return;
        }
    }

    //==========================================================================================================================
    // Health ==================================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 체력을 변경합니다。
    /// </summary>
    /// <param name="amount">변경량</param>
    public void ChangeHealth(int amount)
    {
        if (_data == null)
        {
            return;
        }

        int previousHealth = _data.CurrentHealth;
        
        // 데이터 직접 수정
        _data.CurrentHealth = Mathf.Clamp(_data.CurrentHealth + amount, 0, (int)_data.Health.Value);

        if (previousHealth != _data.CurrentHealth)
        {
            OnHealthChanged?.Invoke(previousHealth, _data.CurrentHealth);
        }
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    public void Die()
    {
        OnDied?.Invoke();
        gameObject.SetActive(false);
    }

    //==========================================================================================================================
    // Stiffness ===============================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 경직도를 추가하고 경직 상태를 결정합니다.
    /// </summary>
    /// <param name="amount">추가할 경직도</param>
    /// <param name="data">데미지 데이터</param>
    public void AddStiffness(int amount)
    {
        ChangeStiffness(amount);
    }

    /// <summary>
    /// 경직도 초기화
    /// </summary>
    public void ResetStiffness()
    {
        ChangeStiffness(-_currentStiffness);
    }

    /// <summary>
    /// 경직도를 변경합니다.
    /// </summary>
    private void ChangeStiffness(int amount)
    {
        int previouseStiffness = _currentStiffness;
        _currentStiffness += amount;

        if (previouseStiffness != _currentStiffness) 
        {
            OnStiffnessChanged?.Invoke(previouseStiffness, _currentStiffness);
        }
    }

    //==========================================================================================================================
    // Heal ====================================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 체력을 회복합니다.
    /// </summary>
    /// <param name="healAmount">회복량</param>
    public void Heal(int healAmount)
    {
        // 죽은 상태면 리턴
        if (IsDead)
        {
            return;
        }

        ChangeHealth(healAmount);
    }

    //==========================================================================================================================
    // Event Handler ===========================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 공격 회복 이벤트 처리
    /// </summary>
    /// <param name="attackRegainedAmount">회복량</param>
    private void OnAttackRegained(int attackRegainedAmount)
    {
        if (IsDead)
        {
            return;
        }

        Heal(attackRegainedAmount);
    }

    /// <summary>
    /// 회복 이벤트 처리
    /// </summary>
    /// <param name="healAmount">회복량</param>
    private void OnHeal(int healAmount)
    {
        if (IsDead)
        {
            return;
        }

        Heal(healAmount);
    }
}