using System;
using UnityEngine;

/// <summary>
/// 플레이어의 피격 타입을 정의하는 열거형입니다.
/// </summary>
public enum PlayerDamagedType
{
    Normal = 0, // 일반 피격
    Strong = 1, // 강한 피격
    KnockDown = 2
}

/// <summary>
/// 플레이어의 모든 이벤트를 관리하고 피드백을 재생하는 클래스입니다.
/// </summary>
public class PlayerEvents : FeedbackPlayer<string>
{
    private PlayerStats _stats;

    #region Events
    public event Action<bool> BattleStateChaged; // 전투 상태 변경 이벤트

    public event Action DodgeStarted, DodgeFinished; // 회피 종료 이벤트

    public event Action AttackStarted, AttackPerformed; // 공격 시작, 공격 수행 이벤트
    public event Action<Collider> AttackAffected; // 공격 타격 이벤트
    public event Action AttackInputWindowStarted, AttackFinished; // 공격 종료 이벤트

    public event Action<Collider> ChargeAttackAffected; // 차지 공격 타격 이벤트

    public event Action ParryWindowStarted, ParryWindowFinished; // 패링 수행 이벤트
    public event Action<Transform> ParrySucceeded; // 패링 성공 이벤트

    public event Action<bool> RegenStamina; // 스테미나 회복 이벤트
    public event Action ChangedNextAttackState; // 다음 공격 상태 변경 이벤트
    #endregion


    public void Initialize(PlayerStats stats)
    {
        _stats = stats;
    }

    #region EventHandler
    /// <summary>
    /// 전투 상태 변경 이벤트를 발생시킵니다.
    /// </summary>
    public void TriggerBattleStateChanged(bool isBattleState)
    {
        BattleStateChaged?.Invoke(isBattleState);
    }

    #region Movement

    /// <summary>
    /// 회피 시작 피드백을 재생합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerDodgeStarted()
    {
        DodgeStarted?.Invoke();
        TriggerRegenStamina(false);
    }

    /// <summary>
    /// 회피 종료 이벤트를 발생시키고 피드백을 재생합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerDodgeFinished()
    {
        DodgeFinished?.Invoke();
        TriggerRegenStamina(true);
    }

    /// <summary>
    /// 착지 피드백을 재생합니다.
    /// </summary>
    public void TriggerLanding()
    {
        // PlayFeedback(PlayerFeedbackType.Landing_FB, transform.position);
    }
    #endregion

    #region Damaged
    /// <summary>
    /// 피격 타입에 맞는 피드백을 재생합니다.
    /// </summary>
    public void TriggerTakeDamged(PlayerDamagedType damagedType)
    {
        switch (damagedType)
        {
            case PlayerDamagedType.Normal: 
                PlayFeedback("NormalHit_FB"); 
                break;
            case PlayerDamagedType.Strong: 
                PlayFeedback("StrongHit_FB"); 
                break;
            case PlayerDamagedType.KnockDown: 
                PlayFeedback("KnockDown_FB");
                break;
        }
    }
    #endregion

 
    #region Attack
    /// <summary>
    /// 공격 시작 피드백을 재생합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerAttackStarted()
    {
        AttackStarted?.Invoke();
        TriggerRegenStamina(false);
    }

    /// <summary>
    /// 공격 수행 이벤트를 발생시킵니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerAttackPerformed()
    {
        AttackPerformed?.Invoke();
    }

    /// <summary>
    /// 근접 공격 타격 이벤트를 발생시키고 피드백을 재생합니다.
    /// </summary>
    public void TriggerAttackAffected(Collider collider)
    {
        AttackAffected?.Invoke(collider);
    }  

    /// <summary>
    /// 공격 중 다음 입력을 받기 시작합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerAttackInputWindowStarted()
    {
        AttackInputWindowStarted?.Invoke();
    }

    /// <summary>
    /// 공격을 끝내고 다음 행동으로 넘어갑니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerAttackFinished()
    {
        AttackFinished?.Invoke();
    }

    #endregion

    #region ChargeAttack
    /// <summary>
    /// 차지 시작 피드백을 재생합니다.  (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerChargeStarted()
    {
        PlayFeedback("ChargeStart_FB");
        TriggerRegenStamina(false);
        TriggerBattleStateChanged(true);
    }

    /// <summary>
    /// 차지 취소 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeCanceled()
    {
        TriggerRegenStamina(true);
        StopFeedback("ChargeStart_FB");
        PlayFeedback("ChargeCancel_FB");
    }

    /// <summary>
    /// 차지 레벨 전환 시 피드백 재생
    /// </summary>
    public void TriggerChargeLevelFeedback(int tier)
    {
        switch (tier)
        {
            case 1:
                PlayFeedback("ChargeLevel1_FB");
                break;
            case 2:
                PlayFeedback("ChargeLevel2_FB");
                break;
            case 3:
                PlayFeedback("ChargeLevel3_FB");
                break;
        }
    }

    /// <summary>
    /// 차지 공격 시작 피드백을 재생합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerChargeAttackFeedbackStarted()
    {
        StopFeedback("ChargeStart_FB");
        switch (_stats.ChargeLevel)
        {
            case 1:
                PlayFeedback("ChargeAttackStartLevel1_FB");
                break;
            case 2:
                PlayFeedback("ChargeAttackStartLevel2_FB");
                break;
            case 3:
                PlayFeedback("ChargeAttackStartLevel3_FB");
                break;
        }
    }

    /// <summary>
    /// 차지 공격 종료 피드백을 재생합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerChargeAttackFinished()
    {
        TriggerRegenStamina(true);
    }

    /// <summary>
    /// 차지 공격 피격 이벤트를 발생시키고 티어에 맞는 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeAttackAffected(Collider collider)
    {
        ChargeAttackAffected?.Invoke(collider);
    }
    #endregion

    #region Parry

    /// <summary>
    /// 패링 검사 시작 (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerParryWindowStarted()
    {
        ParryWindowStarted?.Invoke();
    }

    /// <summary>
    /// 패링 성공 이벤트를 발생시키고 피드백을 재생합니다.
    /// </summary>
    public void TriggerParrySucceeded(Transform transform)
    {
        ParrySucceeded?.Invoke(transform);
        PlayFeedback("Parrying_Sucess_FB", transform.position);
    }

    /// <summary>
    /// 패링 검사 종료 (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerParryWindowFinished()
    {
        ParryWindowFinished?.Invoke();
    }    
    #endregion

    public void TriggerRegenStamina(bool canRegen)
    {
        RegenStamina?.Invoke(canRegen);
    }

    public void TriggerChangedNextAttackState()
    {
        ChangedNextAttackState?.Invoke();
    }

    #endregion
}