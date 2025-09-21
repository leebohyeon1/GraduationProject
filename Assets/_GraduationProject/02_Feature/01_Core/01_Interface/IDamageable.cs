using System;
using UnityEngine;

public interface IDamageable
{
    /// <summary>
    /// 현재 체력
    /// </summary>
    public int Health { get; }

    /// <summary>
    /// 최대 체력
    /// </summary>
    public int MaxHealth { get; }

    /// <summary>
    /// 사망 여부
    /// </summary>
    public bool IsDead { get; }

    /// <summary>
    /// 무적 상태 여부
    /// </summary>
    public bool IsInvincible { get; }  

    /// <summary>
    /// 피격 상태 여부
    /// </summary>
    public bool IsHit { get; }

    /// <summary>
    /// 피해를 받는 함수
    /// </summary>
    /// <param name="damageAmount">피해량</param>
    /// <param name="attacker">피해를 입힌 객체</param>
    public void TakeDamage(int damageAmount, IAttacker attacker = null);

    /// <summary>
    /// 피격 상태 플래그를 리셋 (Hit 상태 종료 시 호출)
    /// </summary>
    void ResetHitState();

    /// <summary>
    /// 체력 변경 이벤트
    /// </summary>
    public event Action<int, int> OnHealthChanged;

    public event Action OnDied;
}
