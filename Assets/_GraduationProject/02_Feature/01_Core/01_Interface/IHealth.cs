using System;
using UnityEngine;

public interface IHealth : IDamageable, IHealable
{
    /// <summary>
    /// 체력을 변경하는 함수
    /// </summary>
    /// <param name="amount">변경량</param>
    public void ChangeHealth(int amount);
}
