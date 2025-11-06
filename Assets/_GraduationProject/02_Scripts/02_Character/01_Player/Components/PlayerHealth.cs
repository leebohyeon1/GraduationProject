using BH_Lib.Log;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

/// <summary>
/// 플레이어의 체력, 데미지 처리, 사망, 무적 상태 등을 관리하는 컴포넌트입니다.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable, IHealable, IStiffness, IDisposable
{
    private PlayerStats _stats; // 플레이어 스탯
    private PlayerEvents _events; // 플레이어 이벤트

    private int _currentStiffness; // 현재 경직도
    private int _stiffnessThreshold = 100; // 경직 임계값
    private float _stiffnessDuration; // 경직 지속 시간
    private float _knockbackForce; // 넉백 힘
    private DamageData _damageData; // 데미지 데이터

    public event Action<bool> OnInvisibleChanged; // 무적 상태 변경 이벤트
    public event Action<int, int> OnHealthChanged; // 체력 변경 이벤트
    public event Action OnDied; // 사망 이벤트

    #region Properties
    public int CurrentStiffness => _currentStiffness; // 현재 경직도
    public int StiffnessThreshold => _stiffnessThreshold; // 경직 임계값
    public float StiffnessDuration => _stiffnessDuration; // 경직 지속 시간
    public float KnockbackForce => _knockbackForce; // 넉백 힘 

    public int Health => _stats.CurrentHealth; // 현재 체력
    public int MaxHealth => _stats.Data.MaxHealth; // 최대 체력
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
        int previousHealth = Health;
        _stats.CurrentHealth = Mathf.Clamp(_stats.CurrentHealth + amount, 0, MaxHealth);

        if (previousHealth != Health)
        {
            OnHealthChanged?.Invoke(previousHealth, Health);
        }
    }

    /// <summary>
    /// 데미지를 받습니다. (경직도 포함)
    /// </summary>
    /// <param name="damageAmount">데미지 양</param>
    /// <param name="stiffenessAmount">경직도 양</param>
    public void TakeDamage(int damageAmount, int heatTier, DamageData damageData)
    {
        if (IsDead || IsInvincible) return;

        _damageData = damageData;

        int stiffenessAmount = damageData.StiffnessAmount;
        if (_stats.IsDefending)
        {
            damageAmount = Mathf.RoundToInt(damageAmount * _stats.Data.CombatData.DefendDamageReductionRate);
            stiffenessAmount = Mathf.RoundToInt(stiffenessAmount * 0.5f);
        }

        AddStiffness(stiffenessAmount);
        ChangeHealth(-damageAmount);

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

        if (_currentStiffness >= _stiffnessThreshold)
        {
            ChangeStiffness(-_currentStiffness); // 경직도 초기화
            HeavyStagger(); // 강한 경직
        }
        else
        {
            LightStagger(); // 약한 경직
        }
    }

    /// <summary>
    /// 경직도를 변경합니다.
    /// </summary>
    private void ChangeStiffness(int amount)
    {
        _currentStiffness += amount;
    }

    /// <summary>
    /// 약한 경직 상태로 전환합니다.
    /// </summary>
    private void LightStagger()
    {
        _damageData.KnockbackCurve = _stats.Data.CombatData.KnockbackCurve;

        if (_stats.IsDefending)
        {
            _stiffnessDuration = _stats.Data.CombatData.DefendStaggerDuration;
            _knockbackForce = _stats.Data.CombatData.DefendKnockbackForce;
        }
        else
        {
            _stiffnessDuration = _stats.Data.CombatData.LightStaggerDuration;
            _knockbackForce = _stats.Data.CombatData.LightKnockbackForce;
        }
    }

    /// <summary>
    /// 강한 경직 상태로 전환합니다.
    /// </summary>
    private void HeavyStagger()
    {
        _stiffnessDuration = DamageData.KnockbackDuration;
        _knockbackForce = DamageData.KnockbackForce;
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
        _damageData = new DamageData();
    }

    public void TakeDamage(DamageData damageData)
    {
        throw new NotImplementedException();
    }
}