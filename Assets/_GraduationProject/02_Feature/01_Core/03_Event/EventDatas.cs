using System;
using UnityEngine;


/// <summary>
/// 체력 변경 이벤트에 사용되는 데이터 구조체입니다.
/// </summary>
[Serializable]
public struct HealthChangeEventData
{
    /// <summary>
    /// 이전 체력
    /// </summary>
    public int PreviousHealth { get; set; }
    /// <summary>
    /// 현재 체력
    /// </summary>
    public int CurrentHealth { get; set; }
    /// <summary>
    /// 최대 체력
    /// </summary>
    public int MaxHealth { get; set; }
    /// <summary>
    /// 데미지를 입었는지 여부
    /// </summary>
    public bool IsDamage => CurrentHealth < PreviousHealth;
    /// <summary>
    /// 치유되었는지 여부
    /// </summary>
    public bool IsHeal => CurrentHealth > PreviousHealth;
    /// <summary>
    /// 현재 체력 비율
    /// </summary>
    public float HealthPercent => (float)CurrentHealth / MaxHealth;
}
