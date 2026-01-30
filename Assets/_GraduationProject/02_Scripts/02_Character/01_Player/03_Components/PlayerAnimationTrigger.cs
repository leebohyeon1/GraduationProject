using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerAnimationTrigger : FeedbackPlayer<string>, IDisposable
{
    private PlayerController p_owner;

    /// <summary>
    /// 컴포넌트 초기화 함수
    /// </summary>
    /// <param name="player">플레이어</param>
    public void Initialize(PlayerController player)
    {
        p_owner = player;

        p_owner.Events.CounterSucceeded += OnCounterSucceeded;
        p_owner.Events.ChargeLevelCompleted += OnChargeLevelCompleted;

        // 이벤트 해제 구독
        player.RegisterDisposable(this);
    }

    /// <summary>
    /// 컴포넌트 해제
    /// </summary>
    public void Dispose()
    {
        p_owner.Events.CounterSucceeded -= OnCounterSucceeded;
        p_owner.Events.ChargeLevelCompleted -= OnChargeLevelCompleted;
    }

    //==========================================================================================================================
    // BufferInput =============================================================================================================
    //==========================================================================================================================

    #region BufferInput
    /// <summary>
    /// 선입력 시작 함수
    /// </summary>
    public void BufferInputStart()
    {
        p_owner.Events.TriggerBufferInputStarted();
    }
    /// <summary>
    /// 선입력 종료 함수
    /// </summary>
    public void BufferInputEnd()
    {
        p_owner.Events.TriggerBufferInputEnded();
    }
    #endregion

    //==========================================================================================================================
    // Dodge ===================================================================================================================
    //==========================================================================================================================

    #region Dodge
    /// <summary>
    /// 회피 시작 함수
    /// </summary>
    public void DodgeStart()
    {
       p_owner.Events.TriggerDodgeStarted();
    }

    #endregion

    //==========================================================================================================================
    // Attack ==================================================================================================================
    //==========================================================================================================================

    #region Attack
    /// <summary>
    /// 공격 시작
    /// </summary>
    public void AttackStart()
    {
        p_owner.Events.TriggerAttackStarted();
    }

    /// <summary>
    /// 공격 타격
    /// </summary>
    public void AttackPerform()
    {
        p_owner.Events.TriggerAttackPerformed();
    }

    /// <summary>
    /// 공격 종료
    /// </summary>
    public void AttackEnd()
    {
        p_owner.Events.TriggerAttackFinished();
    }
    #endregion

    //==========================================================================================================================
    // Counter =================================================================================================================
    //==========================================================================================================================

    #region Counter
    public List<UnityEvent> HeavyCounterFeedbacks;
    public UnityEvent CounterSuccessFeedback;

    /// <summary>
    /// 상쇄 가능 상태 시작
    /// </summary>
    public void EnableCounterWindow()
    {
       p_owner.Events.TriggerCounterWindowStarted();
    }

    /// <summary>
    /// 상쇄 가능 상태 종료
    /// </summary>
    public void DisableCounterWindow()
    {
        p_owner.Events.TriggerCounterWindowFinished();
    }

    /// <summary>
    /// 강한 상쇄 시작
    /// </summary>
    public void HeavyCounterFeedbackPlay()
    {
        HeavyCounterFeedbacks[p_owner.Combat.ChargeLevel]?.Invoke();
    }
    #endregion

    //==========================================================================================================================
    // Charge ==================================================================================================================
    //==========================================================================================================================

    #region Charge
    public List<UnityEvent> ChargeLevelCompletedFeedbacks;
    public UnityEvent ChargeCancelFeedback;


    /// <summary>
    /// 차지 시작 
    /// </summary>
    public void ChargeStarted()
    {
        p_owner.Events.TriggerChargeStarted();  
    }

    public void ChargeCanceled()
    {
        ChargeCancelFeedback?.Invoke();
    }

    #endregion

    //==========================================================================================================================
    // Event Handler ===========================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 상쇄 성공 이벤트 
    /// </summary>
    /// <param name="transform"></param>
    private void OnCounterSucceeded(Transform transform)
    {
        CounterSuccessFeedback?.Invoke();
    }

    /// <summary>
    /// 차지 레벨 달성 이벤트
    /// </summary>
    /// <param name="level">달성한 레벨</param>
    private void OnChargeLevelCompleted(int level)
    {
        ChargeLevelCompletedFeedbacks[level]?.Invoke();
    }

}
