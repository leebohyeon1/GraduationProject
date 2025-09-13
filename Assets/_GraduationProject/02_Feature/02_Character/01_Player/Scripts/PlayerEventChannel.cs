using System;
using BH_Lib.DI;
using UnityEngine;

/// <summary>
/// 플레이어와 관련된 모든 이벤트를 정의하고 관리하는 클래스입니다.
/// </summary>
public class PlayerEventChannel
{
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
        OnRotateToAttackDirection = null;
        OnFootstep = null;
        Dodge.Dispose();
        Parry.Dispose();
        MeleeAttack.Dispose();
        RangedAttack.Dispose();
        ChargeMeleeAttack.Dispose();
        CounterAttack.Dispose();
    }

    #endregion
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