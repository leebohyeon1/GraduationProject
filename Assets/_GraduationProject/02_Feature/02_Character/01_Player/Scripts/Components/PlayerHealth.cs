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

    public event Action<bool> OnInvisibleChanged; // 무적 상태 변경 이벤트
    public event Action<int, int> OnHealthChanged; // 체력 변경 이벤트
    public event Action OnDied; // 사망 이벤트

    #region Properties
    public int CurrentStiffness => _currentStiffness; // 현재 경직도
    public int StiffnessThreshold => _stiffnessThreshold; // 경직 임계값
    public float StiffnessDuration => _stiffnessDuration; // 경직 지속 시간

    public int Health => _stats.CurrentHealth; // 현재 체력
    public int MaxHealth => _stats.MaxHealth; // 최대 체력
    public bool IsDead => _stats.CurrentHealth <= 0; // 사망 여부
    public bool IsInvincible => _stats.IsInvincible; // 무적 여부
    #endregion

    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(PlayerStats data, PlayerEvents evets)
    {
        _stats = data;
        _events = evets;

        _events.OnOverHeat += HandleOverHeat;
    }

    /// <summary>
    /// 리소스 해제 함수
    /// </summary>
    public void Dispose()
    {
        _events.OnOverHeat -= HandleOverHeat;
    }

    /// <summary>
    /// 체력을 변경합니다.
    /// </summary>
    /// <param name="amount">변경량</param>
    public void ChangeHealth(int amount)
    {
        int previousHealth = Health;
        _stats.CurrentHealth = Mathf.Clamp(_stats.CurrentHealth + amount, 0, MaxHealth);

        OnHealthChanged?.Invoke(previousHealth, Health);
    }

    /// <summary>
    /// 데미지를 받습니다. (경직도 없음)
    /// </summary>
    /// <param name="damageAmount">데미지 양</param>
    public void TakeDamage(int damageAmount)
    {
        if (IsDead || IsInvincible) return;

        if (_stats.IsDefending)
        {
            damageAmount = Mathf.RoundToInt(damageAmount * _stats.CombatData.DefendDamageReductionRate);
        }

        ChangeHealth(-damageAmount);

        if (IsDead)
        {
            Die();
        }
    }

    /// <summary>
    /// 데미지를 받습니다. (경직도 포함)
    /// </summary>
    /// <param name="damageAmount">데미지 양</param>
    /// <param name="stiffenessAmount">경직도 양</param>
    public void TakeDamage(int damageAmount, int stiffenessAmount)
    {
        if (IsDead || IsInvincible) return;

        if (_stats.IsDefending)
        {
            damageAmount = Mathf.RoundToInt(damageAmount * _stats.CombatData.DefendDamageReductionRate);
            stiffenessAmount = Mathf.RoundToInt(stiffenessAmount * 0.5f);
        }

        ChangeHealth(-damageAmount);
        AddStiffness(stiffenessAmount);

        if (IsDead)
        {
            Die();
        }
    }

    /// <summary>
    /// 경직도를 추가하고 경직 상태를 결정합니다.
    /// </summary>
    /// <param name="amount">추가할 경직도</param>
    public void AddStiffness(int amount)
    {
        ChangeStiffness(amount);

        if(_currentStiffness >= _stiffnessThreshold)
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
        _stiffnessDuration = _stats.CombatData.LightStaggerDuration;
        _stats.SetDamagedType(PlayerDamagedType.Normal);
    }

    /// <summary>
    /// 강한 경직 상태로 전환합니다.
    /// </summary>
    private void HeavyStagger()
    {
        _stiffnessDuration = _stats.CombatData.HeavyStaggerDuration;
        _stats.SetDamagedType(PlayerDamagedType.Strong);
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
    }

    /// <summary>
    /// 무적 상태를 설정합니다.
    /// </summary>
    public void SetInvisible(bool isInvisible)
    {
        _stats.IsInvincible = isInvisible;
    }

    /// <summary>
    /// 과열 상태일 때 지속적인 데미지를 처리합니다.
    /// </summary>
    private void HandleOverHeat(int damage)
    {
        if(_stats.IsOverHeat)
        {
            TakeDamage(damage);
        }
    }
}