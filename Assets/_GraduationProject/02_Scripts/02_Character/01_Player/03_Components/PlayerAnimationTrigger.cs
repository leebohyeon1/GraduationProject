using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 애니메이션 이벤트와 피드백 재생을 관리하는 컴포넌트입니다.
/// </summary>
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

        if (p_owner.Events != null)
        {
            p_owner.Events.CounterSucceeded += OnCounterSucceeded;
            p_owner.Events.ChargeLevelCompleted += OnChargeLevelCompleted;
            p_owner.Events.BeforeDamaged += OnBeforeDamaged;
        }

        if (p_owner.Health != null)
        {
            p_owner.Health.TakeDamged += OnTakeDamaged;
        }

        if(p_owner.Combat != null)
        {
            p_owner.Combat.ParryStackChanged += OnParryStackChanged;
        }

        // 이벤트 해제 구독 등록
        player.RegisterDisposable(this);
    }

    /// <summary>
    /// 컴포넌트 해제
    /// </summary>
    public void Dispose()
    {
        if (p_owner != null && p_owner.Events != null)
        {
            p_owner.Events.CounterSucceeded -= OnCounterSucceeded;
            p_owner.Events.ChargeLevelCompleted -= OnChargeLevelCompleted;
            p_owner.Events.BeforeDamaged -= OnBeforeDamaged;
        }

        if (p_owner != null && p_owner.Health != null)
        {
            p_owner.Health.TakeDamged -= OnTakeDamaged;
        }

        // 재생 중인 모든 피드백(DOTween) 중단
        DOTween.Kill(this);
        
        Debug.Log("PlayerAnimationTrigger: 리소스가 성공적으로 해제되었습니다.");
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
    public UnityEvent ClashDodgeFeedback;

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

    /// <summary>
    /// 다음 전투 상태로 전환
    /// </summary>
    public void ChangeNextCombatState()
    {
        p_owner.Events.TriggerChangeNextCombatState();
    }

    #endregion

    //==========================================================================================================================
    // Counter =================================================================================================================
    //==========================================================================================================================

    #region Counter
    public List<UnityEvent> HeavyCounterFeedbacks;
    public UnityEvent CounterSuccessFeedback;
    public List<UnityEvent> ParryStackChangeFeedbacks;

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

    /// <summary>
    /// 투사체 상쇄 체크
    /// </summary>
    public void CheckProjectileCounter()
    {
        p_owner.Combat.TriggerCheckedProjectileCounter();
    }

    /// <summary>
    /// 패리 스택 변경 이벤트
    /// </summary>
    /// <param name="obj">스택</param>
    private void OnParryStackChanged(int obj)
    {
        ParryStackChangeFeedbacks[obj]?.Invoke();
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
        if (ChargeLevelCompletedFeedbacks != null && level >= 0 && level < ChargeLevelCompletedFeedbacks.Count)
        {
            ChargeLevelCompletedFeedbacks[level]?.Invoke();
        }
    }

    /// <summary>
    /// 플레이어가 데미지 받았을 때 이벤트
    /// </summary>
    /// <param name="damage">데미지</param>
    private void OnTakeDamaged(int damage)
    {
        if (p_owner != null && p_owner.Ability != null && p_owner.Ability.HasTag("Clash_IncreaseDamageReduction"))
        {
            ClashDodgeFeedback?.Invoke();
        }
    }

    //==========================================================================================================================
    // Damaged =================================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 데미지 받기 전 이벤트 발행
    /// </summary>
    /// <param name="damageContext">받은 데미지 데이터</param>
    private void OnBeforeDamaged(ref PlayerDamageContext damageContext)
    {
        if(p_owner != null && p_owner.Ability != null && p_owner.Ability.HasTag("SuperArmor"))
        {
            PlayFeedback("SuperArmor_Damage_FB");
        }
    }

    //==========================================================================================================================
    // Damaged =================================================================================================================
    //==========================================================================================================================

    public void Land()
    {
        p_owner.Events.TriggerLanded();
    }
}
