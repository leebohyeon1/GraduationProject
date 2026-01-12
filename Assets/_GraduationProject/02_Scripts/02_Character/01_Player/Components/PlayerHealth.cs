using System;
using UnityEngine;

/// <summary>
/// 플레이어의 체력, 데미지 처리, 사망, 무적 상태 등을 관리하는 컴포넌트입니다.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable, IHealable, IStiffness, IDisposable
{
    private PlayerStats _stats; // 플레이어 스탯
    private PlayerEvents _events; // 플레이어 이벤트

    private int _currentStiffness; // 현재 경직도
    private float _stiffnessDuration; // 경직 지속 시간
    private float _knockbackForce; // 넉백 힘
    private DamageData _damageData; // 데미지 데이터

    public event Action<bool> OnInvisibleChanged; // 무적 상태 변경 이벤트
    public event Action<int, int> OnHealthChanged; // 체력 변경 이벤트
    public event Action OnDied; // 사망 이벤트
    public event Action<int, int> OnStiffnessChanged;

    #region Properties
    public int CurrentStiffness => _currentStiffness; // 현재 경직도
    public int StiffnessThreshold => 100; // 경직 임계값
    public float StiffnessDuration => _stiffnessDuration; // 경직 지속 시간
    public float KnockbackForce => _knockbackForce; // 넉백 힘 

    public int Health => _stats.CurrentHealth; // 현재 체력
    public int MaxHealth => _stats.RuntimeData.MaxHealth; // 최대 체력
    public bool IsDead => _stats.CurrentHealth <= 0; // 사망 여부
    public bool IsInvincible => _stats.IsInvincible; // 무적 여부

    public DamageData DamageData => _damageData; // 데미지 데이터
    #endregion

    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(PlayerStats data, PlayerEvents evets)
    {
        _stats = data;
        _events = evets;

        OnHealthChanged?.Invoke(Health, Health);
        OnStiffnessChanged?.Invoke(0, 0);

        _events.DodgeStarted += OnDodgeStarted;
    }

    /// <summary>
    /// 리소스 해제 함수
    /// </summary>
    public void Dispose()
    {
        _events.DodgeStarted -= OnDodgeStarted;
    }

    /// <summary>
    /// 체력을 변경합니다.
    /// </summary>
    /// <param name="amount">변경량</param>
    public void ChangeHealth(int amount)
    {
        int previousHealth = Health;
        _stats.CurrentHealth = Mathf.Clamp(_stats.CurrentHealth + amount, 0, MaxHealth);

        if (previousHealth != Health)
        {
            OnHealthChanged?.Invoke(previousHealth, Health);
        }
    }

    public void TakeDamage(DamageData damageData)
    {
        if (IsDead || IsInvincible) return;

        Vector3 toEnemy = damageData.AttackerTransform.transform.position - transform.position;

        if(damageData.AttackType == AttackType.Heavy && _stats.IsChage)
        {
            if (_stats.IsParring && Vector3.Angle(transform.forward, toEnemy) <= (_stats.RuntimeData.CombatData.ParryAngle / 2f)
             && damageData.AttackerTransform.TryGetComponent<IParryable>(out IParryable parryable))
                {
                    if (!_stats.ParrySet.Contains(parryable))
                    {
                        _events.TriggerParrySucceeded(damageData.AttackerTransform);
                        _stats.ParrySet.Add(parryable);
                    }

                   Debug.Log("상쇄");
                    parryable.Parry(damageData.AttackType);
                    return;
                }
        }
        else if(damageData.AttackType != AttackType.Heavy)
        {
            if (_stats.IsParring && Vector3.Angle(transform.forward, toEnemy) <= (_stats.RuntimeData.CombatData.ParryAngle / 2f)
            && damageData.AttackerTransform.TryGetComponent<IParryable>(out IParryable parryable))
            {
                if (!_stats.ParrySet.Contains(parryable))
                {
                    _events.TriggerParrySucceeded(damageData.AttackerTransform);
                    _stats.ParrySet.Add(parryable);
                }

                Debug.Log("상쇄");
                parryable.Parry(damageData.AttackType);
                return;
            }
        }

        _damageData = damageData;

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

            KnockDown();
            return;
        }

        switch(DamageData.AttackType)
        {
            case AttackType.Normal:
                MiddleStagger(); // 약한 경직
                break;
            case AttackType.Range:
                MiddleStagger(); // 약한 경직
                break;  
            case AttackType.Heavy:
                HeavyStagger(); // 강한 경직
                break;
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

    /// <summary>
    /// 약한 경직 상태로 전환합니다.
    /// </summary>
    private void MiddleStagger()
    {
        _stats.IsMiddleHit = true;
        _damageData.KnockbackCurve = _stats.RuntimeData.CombatData.KnockbackCurve;
        _stiffnessDuration = _stats.RuntimeData.CombatData.MiddleStaggerDuration;
        _knockbackForce = _stats.RuntimeData.CombatData.MiddleKnockbackForce;
    }

    /// <summary>
    /// 강한 경직 상태로 전환합니다.
    /// </summary>
    private void HeavyStagger()
    {
        _stats.IsHeavyHit = true; 
        _stiffnessDuration = DamageData.KnockbackDuration;
        _knockbackForce = DamageData.KnockbackForce;
    }

    private void KnockDown()
    {
        _stats.IsKnockDown = true;
        _stiffnessDuration = 1.5f;
        _knockbackForce = 0f;
    }

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

    /// <summary>
    /// 무적 상태를 설정합니다.
    /// </summary>
    public void SetInvisible(bool isInvisible)
    {
        _stats.IsInvincible = isInvisible;
    }

    /// <summary>
    /// 데미지 데이터 초기화
    /// </summary>
    public void ResetDamageData()
    {
        _stats.IsMiddleHit = false;
        _stats.IsHeavyHit = false;
        _stats.IsKnockDown = false;
        _damageData = new DamageData();
    }


    #region EventHandlers
    /// <summary>
    /// 구르기 시작 이벤트 핸들러
    /// </summary>
    private void OnDodgeStarted()
    {
        SetInvisible(true); // 회피 중 무적
    }

    #endregion
}