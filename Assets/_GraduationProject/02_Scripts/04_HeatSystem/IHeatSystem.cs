using System;
using UnityEngine;

public interface IHeatable
{
    /// <summary>
    /// 오브젝트 타입
    /// </summary>
    public ActorType ActorType{ get; }
    /// <summary>
    /// 최대 열량
    /// </summary>
    public int MaxHeat { get; }
    /// <summary>
    /// 현재 열량
    /// </summary>
    public int CurrentHeat { get; }
    /// <summary>
    /// 현재 열기 티어
    /// </summary>
    public int CurrentTier { get; }
    /// <summary>
    /// 열기 변화 가능 여부
    /// </summary>
    public bool IsHeatLock { get; }

    /// <summary>
    /// 열기량이 바뀌었을 때 이벤트
    /// </summary>
    public event Action<int, int> OnHeatChanged;
    /// <summary>
    /// 티어가 바뀌었을 때 이벤트
    /// </summary>
    public event Action<int, int> OnTierChanged;
    
    /// <summary>
    /// 열량을 바꾸는 함수
    /// </summary>
    /// <param name="amount">바꿀 열량</param>
    public void ChangeHeat(int amount);
    /// <summary>
    /// 열기 변화 가능 여부 설정 함수
    /// </summary>
    /// <param name="isLock">잠금 여부</param>
    public void SetHeatLock(bool isLock);
    /// <summary>
    /// 현재 티어 가져오는 함수
    /// </summary>
    /// <returns></returns>
    public int GetTier();
}

public interface IOverHeatable
{
    public int TriggerThrehold { get; }
    public float DelaySecond { get; }
    public float TickSecond { get; }
    public int DamagePerTick { get; }
    public int MaxHpRatioDamage { get; }
    public float GroggySecond { get; }
    public bool IsHeatLock { get; }

    public void OverHeat();
}
