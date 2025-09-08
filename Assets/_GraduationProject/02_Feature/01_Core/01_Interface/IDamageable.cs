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
    /// 체력이 변경될 때 발생하는 이벤트
    /// </summary>
    public event Action<int, int> OnHealthChanged;

    /// <summary>
    /// 사망했을 때 발생하는 이벤트
    /// </summary>
    public event Action OnDeath;

    /// <summary>
    /// 피해를 받는 함수
    /// </summary>
    /// <param name="damageAmount">피해량</param>
    /// <param name="attacker">피해를 입힌 객체</param>
    public void TakeDamage(int damageAmount, IAttacker attacker = null);
}
