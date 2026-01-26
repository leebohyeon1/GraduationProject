using System;
using UnityEngine;

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

        OnHealthChanged?.Invoke(CurrentHealth, CurrentHealth);
        OnStiffnessChanged?.Invoke(0, 0);
    }

    /// <summary>
    /// 리소스 해제 함수
    /// </summary>
    public void Dispose()
    {
    }

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

    public void TakeDamage(DamageData damageData)
    {
        if (IsDead)
        {
            return;
        }

        Vector3 toEnemy = damageData.AttackerTransform.transform.position - transform.position;

        if (damageData.AttackType != AttackType.Absoluteness)
        {
            if(damageData.AttackType <= AttackType.NormalCounter) 
            {
                //if (_stats.IsCounterable && Vector3.Angle(transform.forward, toEnemy) <= (_stats.RuntimeData.CombatData.CounterAngle / 2f)
                //&& damageData.AttackerTransform.TryGetComponent<IParryable>(out IParryable parryable) && damageData.AttackType <= AttackType.NormalCounter + _stats.ChargeLevel)
                //{
                //    if (!_stats.CounterEnemySet.Contains(parryable))
                //    {
                //        _events.TriggerParrySucceeded(damageData.AttackerTransform);
                //        _stats.CounterEnemySet.Add(parryable);
                //    }

                //    Debug.Log("상쇄");
                //    parryable.Parry(AttackType.NormalCounter + _stats.ChargeLevel);
                //    return;
                //}
            }
        }

        AddStiffness(damageData.StiffnessAmount);
        ChangeHealth(-damageData.DamageAmount);

        if (IsDead)
        {
            Die();
        }
    }

    /// <summary>
    /// 경직도를 추가하고 경직 상태를 결정합니다.
    /// </summary>
    /// <param name="amount">추가할 경직도</param>
    /// <param name="data">데미지 데이터</param>
    public void AddStiffness(int amount)
    {
        ChangeStiffness(amount);

        if(CurrentStiffness >= StiffnessThreshold)
        {
            ChangeStiffness(-_currentStiffness); // 경직도 초기화

            return;
        }


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

    ///// <summary>
    ///// 약한 경직 상태로 전환합니다.
    ///// </summary>
    //private void MiddleStagger()
    //{
    //    _stats.IsMiddleHit = true;
    //    _stiffnessDuration = DamageData.KnockbackDuration;
    //    _knockbackForce = DamageData.KnockbackForce;
    //}

    ///// <summary>
    ///// 강한 경직 상태로 전환합니다.
    ///// </summary>
    //private void HeavyStagger()
    //{
    //    _stats.IsHeavyHit = true; 
    //    _stiffnessDuration = DamageData.KnockbackDuration;
    //    _knockbackForce = DamageData.KnockbackForce;
    //}

    //private void KnockDown()
    //{
    //    _stats.IsKnockDown = true;
    //    _stiffnessDuration = 1.5f;
    //    _knockbackForce = 0f;
    //}

    /// <summary>
    /// 체력을 회복합니다.
    /// </summary>
    /// <param name="healAmount">회복량</param>
    public void Heal(int healAmount)
    {
        if (IsDead) return;
        ChangeHealth(healAmount);
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    public void Die()
    {
        OnDied?.Invoke();
        gameObject.SetActive(false);
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
}