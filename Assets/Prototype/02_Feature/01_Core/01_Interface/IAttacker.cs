using UnityEngine;

public interface IAttacker
{
    /// <summary>
    /// 공격력
    /// </summary>
    float AttackDamage { get; }

    /// <summary>
    /// 공격 속도
    /// </summary>
    float AttackSpeed { get; }

    /// <summary>
    /// 대상을 공격하는 함수
    /// </summary>
    /// <param name="target">공격할 대상</param>
    void Attack(IDamageable target);
}