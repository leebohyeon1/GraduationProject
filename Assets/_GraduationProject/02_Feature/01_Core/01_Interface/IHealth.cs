using System;
using UnityEngine;

public interface IHealth : IDamageable, IHealable
{
    /// <summary>
    /// 체력을 변경하는 함수
    /// </summary>
    /// <param name="amount">변경량</param>
    public void ChangeHealth(int amount);

    /// <summary>
    /// 죽음 처리 함수
    /// </summary>
    public void Die();

    /// <summary>
    /// 무적 상태 설정
    /// </summary>
    /// <param name="isInvisible">무적 상태 여부</param>
    public void SetInvisible(bool isInvisible);

    /// <summary>
    /// 무적 상태 변경 이벤트
    /// </summary>
    public event Action<bool> OnInvisibleChanged;
}
