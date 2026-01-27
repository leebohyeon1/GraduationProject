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

    [Header("Health")]
    private int _maxHealth;         // 최대 체력
    private int _currentHealth;     // 현재 체력

    private float _damageReduction; // 데미지 감소량

    public event Action<int, int> OnHealthChanged; // 체력 변경 이벤트
    public event Action OnDied; // 사망 이벤트

    [Header("Shield")]
    private int _currentshieldAmount;   // 현재 보호막 양

    [Header("Stiffness")]
    private int _currentStiffness; // 현재 경직도
    private float _stiffnessDuration; // 경직 지속 시간

    public event Action<int, int> OnStiffnessChanged;

    [Header("Properties")]
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public bool IsDead => CurrentHealth <= 0; // 사망 여부

    public int CurrentStiffness => _currentStiffness; // 현재 경직도
    public int StiffnessThreshold => 100; // 경직 임계값
    public float StiffnessDuration => _stiffnessDuration; // 경직 지속 시간


    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(PlayerController player)
    {
        _events = player.Events;

        // 체력 초기화
        _maxHealth = player.Data.MaxHealth;
        _currentHealth = _maxHealth;

        // 경직도 초기화
        _currentStiffness = 0;

        // 이벤트 해제 구독
        player.RegisterDisposable(this);
    }

    /// <summary>
    /// 리소스 해제 함수
    /// </summary>
    public void Dispose()
    {
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

        // 보호막 양만큼 데미지 감소
        int shieldDamage = 0;
        if(damageData.DamageAmount >= _currentshieldAmount)
        {
            shieldDamage = _currentshieldAmount;
        }
        else
        {
            shieldDamage = damageData.DamageAmount;    
        }

        // 실드 피해 만큼 데미지 양에서 제거
        damageData.DamageAmount = Mathf.Max(damageData.DamageAmount - shieldDamage, 0); 
        DecreaseShield(shieldDamage);               // 보호막 감소

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
    /// 체력을 변경합니다.
    /// </summary>
    /// <param name="amount">변경량</param>
    public void ChangeHealth(int amount)
    {
        int previousHealth = CurrentHealth;
        _currentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);

        if (previousHealth != CurrentHealth)
        {
            OnHealthChanged?.Invoke(previousHealth, CurrentHealth);
        }
    }

    #region DamageReduction Management
    /// <summary>
    /// 데미지 감소량 설정
    /// </summary>
    /// <param name="value">설정할 값</param>
    public void SetDamageReduction(float value)
    {
        _damageReduction = value;
    }

    /// <summary>
    /// 데미지 감소량 초기화
    /// </summary>
    public void ResetDamageReduction()
    {
        _damageReduction = 0f;
    }

    /// <summary>
    /// 데미지 감소량 증가
    /// </summary>
    /// <param name="value">증가량</param>
    public void IncreaseDamageReduction(float value)
    {
        _damageReduction += value;
    }

    /// <summary>
    /// 데미지 감소량 감소
    /// </summary>
    /// <param name="value">감소량</param>
    public void DecreaseDamageReduction(float value)
    {
        _damageReduction -= value;
    }
    #endregion

    /// <summary>
    /// 사망 처리
    /// </summary>
    public void Die()
    {
        OnDied?.Invoke();
        gameObject.SetActive(false);
    }

    //==========================================================================================================================
    // Shiled ==================================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 보호막 양 증가
    /// </summary>
    /// <param name="shieldAmount">보호막 양</param>
    public void IncreaseShield(int  shieldAmount)
    {
        _currentshieldAmount += shieldAmount;
    }

    /// <summary>
    /// 보호막 양 감소
    /// </summary>
    /// <param name="shieldAmount">보호막 양</param>
    public void DecreaseShield(int shieldAmount)
    {
        _currentshieldAmount -= shieldAmount;

        if(_currentshieldAmount <= 0)
        {
            _currentshieldAmount = 0;
        }
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
        if (IsDead) return;
        ChangeHealth(healAmount);
    }

}