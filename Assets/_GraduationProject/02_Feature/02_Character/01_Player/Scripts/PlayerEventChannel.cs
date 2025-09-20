using System;
using System.Collections.Generic;
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
    /// 플레이어가 멈출때 발생하는 이벤트입니다.
    /// </summary>
    public event Action OnMoveStop;
    /// <summary>
    /// 플레이어가 착지했을 때 발생하는 이벤트입니다.
    /// </summary>
    public event Action OnLand;
    /// <summary>
    /// 플레이어 회피 액션 관련 이벤트입니다.
    /// </summary>
    public PlayerActionEvents<int> Dodge;
    /// <summary>
    /// 플레이어 패링 액션 관련 이벤트입니다.
    /// </summary>
    public PlayerActionEvents<Collider> Parry;
    /// <summary>
    /// 플레이어가 약한 피격되었을 때 발생하는 이벤트입니다.
    /// </summary>
    public event Action OnNomalHit;
    /// <summary>
    /// 플레이어가 강한 피격되었을 때 발생하는 이벤트입니다.
    /// </summary>
    public event Action OnStrongHit;
    /// <summary>
    /// 플레이어가 방어에 성공했을 때 발생하는 이벤트입니다.
    /// </summary>
    public event Action OnDefendHit;

    // 플레이어 공격 관련 이벤트
    /// <summary>
    /// 플레이어 근접 공격 액션 관련 이벤트입니다.
    /// </summary>
    public PlayerActionEvents<Collider> MeleeAttack;
    public Action<Vector3> OnFirstMeleeAttackEffect;
    public Action<Vector3> OnSecondMeleeAttackEffect;
    public Action<Vector3> OnThirdMeleeAttackEffect;

    /// <summary>
    /// 플레이어 차지 근접 공격 액션 관련 이벤트입니다.
    /// </summary>
    public PlayerActionEvents<Collider> ChargeMeleeAttack;
    public event Action<Vector3> OnTier1ChargeAttackEffect;
    public event Action<Vector3> OnTier2ChargeAttackEffect;
    public event Action<Vector3> OnTier3ChargeAttackEffect;
    public PlayerChargeActionEvents MeleeAttackCharge;

    /// <summary>
    /// 플레이어 원거리 공격 액션 관련 이벤트입니다.
    /// </summary>
    public PlayerActionEvents<Collider> RangedAttack;
    public PlayerChargeActionEvents RangedAttackCharge;

    /// <summary>
    /// 플레이어 카운터 공격 액션 관련 이벤트입니다.
    /// </summary>
    public PlayerActionEvents<Collider> CounterAttack;
    public event Action<Vector3> OnTier1FirstCounterAttackEffect;
    public event Action<Vector3> OnTier2FirstCounterAttackEffect;
    public event Action<Vector3> OnTier3FirstCounterAttackEffect;
    public event Action<Vector3> OnTier1SecondCounterAttackEffect;
    public event Action<Vector3> OnTier2SecondCounterAttackEffect;
    public event Action<Vector3> OnTier3SecondCounterAttackEffect;
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
    /// 멈춤 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishMoveStop()
    {
        OnMoveStop?.Invoke();
    }

    /// <summary>
    /// 착지 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishLand()
    {
        OnLand?.Invoke();
    }

    /// <summary>
    /// 피격 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishNormalHit()
    {
        OnNomalHit?.Invoke();
    }

    /// <summary>
    /// 강한 피격 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishStrongHit()
    {
        OnStrongHit?.Invoke();
    }   

    /// <summary>
    /// 방어 성공 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishDefendHit()
    {
        OnDefendHit?.Invoke();
    }

    /// <summary>
    /// 근접 공격 이펙트 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishMeleeAttackEffect(int comboStep, Vector3 position)
    {
        if (comboStep == 1)
        {
            OnFirstMeleeAttackEffect?.Invoke(position);
        }
        else if (comboStep == 2)
        {
            OnSecondMeleeAttackEffect?.Invoke(position);
        }
        else if (comboStep == 3)
        {
            OnThirdMeleeAttackEffect?.Invoke(position);
        }
        else
        {
            OnFirstMeleeAttackEffect?.Invoke(position); 
        } 
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
    public event Action<Vector3> OnStart;
    /// <summary>
    /// 액션이 수행될 때 발생합니다.
    /// </summary>
    public event Action<Vector3> OnPerform;
    /// <summary>
    /// 액션이 끝났을 때 발생합니다.
    /// </summary>
    public event Action<Vector3> OnFinished;
    /// <summary>
    /// 액션이 취소되었을 때 발생합니다.
    /// </summary>
    public event Action OnCancel;
    /// <summary>
    /// 액션이 대상에 영향을 미쳤을 때 발생합니다.
    /// </summary>
    public event Action<Vector3, T> OnAffect;

    /// <summary>
    /// 액션 시작 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishStart(Vector3 position)
    {
        OnStart?.Invoke(position);
    }
    /// <summary>
    /// 액션 수행 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishPerform(Vector3 position)
    {
        OnPerform?.Invoke(position);
    }
    /// <summary>
    /// 액션 종료 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishFinished(Vector3 position)
    {
        OnFinished?.Invoke(position);
    }
    /// <summary>
    /// 액션 취소 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishCancel()
    {
        OnCancel?.Invoke();
    }
    /// <summary>
    /// 액션 영향 이벤트를 발생시킵니다.
    /// </summary>
    /// <param name="targets">영향을 받은 대상</param>
    public void PublishAffect(Vector3 position, T targets)
    {
        OnAffect?.Invoke(position, targets);
    }
}

/// <summary>
/// 차지 가능한 액션에 특화된 이벤트 그룹 구조체입니다.
/// </summary>
public struct PlayerChargeActionEvents
{
    /// <summary>
    /// 차징이 시작될 때 발생합니다.
    /// </summary>
    public event Action<Vector3> OnStart;
    /// <summary>
    /// 차징 효과가 발생할 때 발생합니다.
    /// </summary>
    public event Action<Vector3> OnPerform;
    /// <summary>
    /// 차징이 끝났을 때 발생합니다.
    /// </summary>
    public event Action<Vector3> OnFinished;
    /// <summary>
    /// 차징이 취소되었을 때 발생합니다.
    /// </summary>
    public event Action<Vector3> OnCancel;

    /// <summary>
    /// 차징 시작 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishStart(Vector3 postion) => OnStart?.Invoke(postion);
    /// <summary>
    /// 차징 효과 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishPerform(Vector3 postion) => OnPerform?.Invoke(postion);
    /// <summary>
    /// 차징 종료 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishFinished(Vector3 postion) => OnFinished?.Invoke(postion);
    /// <summary>
    /// 차징 취소 이벤트를 발생시킵니다.
    /// </summary>
    public void PublishCancel(Vector3 postion) => OnCancel?.Invoke(postion);
}