using System;
using BH_Lib.DI;
using UnityEngine;


// 이벤트 우선순위 정의
public enum EventPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3  // 전투 중 즉시 반응이 필요한 이벤트
}

// 기본 이벤트 인터페이스
public interface IGameEvent
{
    EventPriority Priority { get; }
    float TimeStamp { get; }
}

[Register(LifetimeScope.Singleton)]
public class EventManager : MonoBehaviour
{
    public PlayerEventChannel Player { get; private set; }

    private void Awake()
    {
        Player = new PlayerEventChannel();
    }
}

public class PlayerEventChannel
{
    // 플레이어 체력 관련 이벤트
    public event Action<HealthChangeEventData> OnHealthChanged;
    public event Action OnDied;

    // 플레이어 움직임 관련 이벤트
    public event Action<InputDeviceType, Vector2, Vector2> OnRotateToAttackDirection;
    public event Action OnFootstep;
    public PlayerActionEvents<int> Dodge;
    public PlayerActionEvents<Collider> Parry;


    // 플레이어 공격 관련 이벤트
    public PlayerActionEvents<Collider> MeleeAttack;
    public PlayerActionEvents<Collider> ChargeMeleeAttack;
    public PlayerActionEvents<Collider> RangedAttack;


    #region Public Methods
    public void PublishHealthChanged(HealthChangeEventData healthEvent)
    {
        OnHealthChanged?.Invoke(healthEvent);
    }

    public void PublishPlayerDied()
    {
        OnDied?.Invoke();
    }

    public void PublishRotateToAttackDirection(InputDeviceType deviceType, Vector2 lookInput, Vector2 mousePosition)
    {
        OnRotateToAttackDirection?.Invoke(deviceType, lookInput, mousePosition);
    }

    public void PublishFootstep()
    {
        OnFootstep?.Invoke();
    }

    public void Dispose()
    {
        OnHealthChanged = null;
        OnDied = null;
        OnRotateToAttackDirection = null;
        OnFootstep = null;
        Dodge.Dispose();
        Parry.Dispose();
        MeleeAttack.Dispose();
        RangedAttack.Dispose();
        ChargeMeleeAttack.Dispose();
    }
    
    #endregion
}


[Serializable]
public struct HealthChangeEventData : IGameEvent
{
    public int PreviousHealth { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public bool IsDamage => CurrentHealth < PreviousHealth;
    public bool IsHeal => CurrentHealth > PreviousHealth;
    public float HealthPercent => (float)CurrentHealth / MaxHealth;

    public EventPriority Priority => EventPriority.Critical;
    public float TimeStamp { get; set; }
}

public struct PlayerActionEvents<T>
{
    public event Action OnStart;
    public event Action OnPerform;
    public event Action OnFinished;
    public event Action OnCharge;
    public event Action<T> OnAffect;

    public void PublishStart()
    {
        OnStart?.Invoke();
    }
    public void PublishPerform()
    {
        OnPerform?.Invoke();
    }
    public void PublishFinished()
    {
        OnFinished?.Invoke();
    }
    public void PublishCharge()
    {
        OnCharge?.Invoke();
    }
    public void PublishAffect(T targets)
    {
        OnAffect?.Invoke(targets);
    }

    public void Dispose()
    {
        OnStart = null;
        OnPerform = null;
        OnFinished = null;
        OnCharge = null;
        OnAffect = null;
    }
}
