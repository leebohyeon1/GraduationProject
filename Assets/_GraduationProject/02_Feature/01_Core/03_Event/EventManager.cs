using System;
using BH_Lib.DI;
using UnityEngine;

/// <summary>
/// 게임 내 모든 이벤트를 관리하는 싱글톤 클래스입니다.
/// 다양한 이벤트 채널을 소유하고 초기화합니다.
/// </summary>
[Register(LifetimeScope.Singleton)]
public class EventManager : MonoBehaviour
{
    /// <summary>
    /// 플레이어 관련 이벤트를 관리하는 채널입니다.
    /// </summary>
    public PlayerEventChannel Player { get; private set; }

    private void Awake()
    {
        Player = new PlayerEventChannel();
    }
}

/// <summary>
/// 플레이어와 관련된 모든 이벤트를 정의하고 관리하는 클래스입니다.
/// </summary>
public class PlayerEventChannel
{
    // 플레이어 체력 관련 이벤트
    /// <summary>
    /// 플레이어의 체력이 변경될 때 발생하는 이벤트입니다.
    /// </summary>
    public event Action<HealthChangeEventData> OnHealthChanged;
    /// <summary>
    /// 플레이어가 사망했을 때 발생하는 이벤트입니다.
    /// </summary>
    public event Action OnDied;

    // 플레이어 움직임 관련 이벤트
    /// <summary>
    /// 플레이어가 공격 방향으로 회전할 때 발생하는 이벤트입니다.
    /// </summary>
    public event Action<InputDeviceType, Vector2, Vector2> OnRotateToAttackDirection;
    /// <summary>
    /// 플레이어 발소리가 날 때 발생하는 이벤트입니다.
    /// </summary>
    public event Action OnFootstep;
    /// <summary>
    /// 플레이어 회피 액션 관련 이벤트입니다.
    /// </summary>
    public PlayerActionEvents<int> Dodge;
    /// <summary>
    /// 플레이어 패링 액션 관련 이벤트입니다.
    /// </summary>
    public PlayerActionEvents<Collider> Parry;


    // 플레이어 공격 관련 이벤트
    /// <summary>
    /// 플레이어 근접 공격 액션 관련 이벤트입니다.
    /// </summary>
    public PlayerActionEvents<Collider> MeleeAttack;
    /// <summary>
    /// 플레이어 차지 근접 공격 액션 관련 이벤트입니다.
    /// </summary>
    public PlayerActionEvents<Collider> ChargeMeleeAttack;
    /// <summary>
    /// 플레이어 원거리 공격 액션 관련 이벤트입니다.
    /// </summary>
    public PlayerActionEvents<Collider> RangedAttack;
    /// <summary>
    /// 플레이어 카운터 공격 액션 관련 이벤트입니다.
    /// </summary>
    public PlayerActionEvents<Collider> CounterAttack;


    #region Public Methods
    /// <summary>
    /// 체력 변경 이벤트를 발생시킵니다.
    /// </summary>
    /// <param name="healthEvent">체력 변경 데이터</param>
    public void PublishHealthChanged(HealthChangeEventData healthEvent)
    {
        OnHealthChanged?.Invoke(healthEvent);
    }

    /// <summary>
    /// 플레이어 사망 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishPlayerDied()
    {
        OnDied?.Invoke();
    }

    /// <summary>
    /// 공격 방향으로 회전 이벤트를 발생시킵니다.
    /// </summary>
    /// <param name="deviceType">입력 장치 종류</param>
    /// <param name="lookInput">바라보는 방향 입력</param>
    /// <param name="mousePosition">마우스 위치</param>
    public void PublishRotateToAttackDirection(InputDeviceType deviceType, Vector2 lookInput, Vector2 mousePosition)
    {
        OnRotateToAttackDirection?.Invoke(deviceType, lookInput, mousePosition);
    }

    /// <summary>
    /// 발소리 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishFootstep()
    {
        OnFootstep?.Invoke();
    }

    /// <summary>
    /// 모든 이벤트 구독을 해제합니다.
    /// </summary>
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

/// <summary>
/// 플레이어의 일반적인 액션(시작, 수행, 종료 등)에 대한 이벤트를 그룹화한 제네릭 구조체입니다.
/// </summary>
/// <typeparam name="T">이벤트 발생 시 전달할 데이터 타입</typeparam>
public struct PlayerActionEvents<T>
{
    /// <summary>
    /// 액션이 시작될 때 발생합니다.
    /// </summary>
    public event Action OnStart;
    /// <summary>
    /// 액션이 수행될 때 발생합니다.
    /// </summary>
    public event Action OnPerform;
    /// <summary>
    /// 액션이 끝났을 때 발생합니다.
    /// </summary>
    public event Action OnFinished;
    /// <summary>
    /// 액션이 차지(충전)될 때 발생합니다.
    /// </summary>
    public event Action OnCharge;
    /// <summary>
    /// 액션이 대상에 영향을 미쳤을 때 발생합니다.
    /// </summary>
    public event Action<T> OnAffect;

    /// <summary>
    /// 액션 시작 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishStart()
    {
        OnStart?.Invoke();
    }
    /// <summary>
    /// 액션 수행 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishPerform()
    {
        OnPerform?.Invoke();
    }
    /// <summary>
    /// 액션 종료 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishFinished()
    {
        OnFinished?.Invoke();
    }
    /// <summary>
    /// 액션 차지 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishCharge()
    {
        OnCharge?.Invoke();
    }
    /// <summary>
    /// 액션 영향 이벤트를 발생시킵니다.
    /// </summary>
    /// <param name="targets">영향을 받은 대상</param>
    public void PublishAffect(T targets)
    {
        OnAffect?.Invoke(targets);
    }

    /// <summary>
    /// 모든 이벤트 구독을 해제합니다.
    /// </summary>
    public void Dispose()
    {
        OnStart = null;
        OnPerform = null;
        OnFinished = null;
        OnCharge = null;
        OnAffect = null;
    }
}