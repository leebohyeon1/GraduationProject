using BH_Lib.AssetManager;
using BH_Lib.Log;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 플레이어의 피드백(이펙트, 사운드 등) 타입을 정의하는 열거형입니다.
/// </summary>
public enum PlayerFeedbackType
{
    Move_FB, MoveStop_FB, DodgeStart_FB,
    DodgeFinish_FB, Landing_FB,

    TakeDamage_Normal_FB, TakeDamage_Strong_FB, TakeDamage_Defend_FB, TakeDamage_KnockDown_FB,

    FirstAttackStart_FB, SecondAttackStart_FB,
    ThirdAttackStart_FB, MeleeAttackHit_FB,

    ChargeStart_FB, ChargeCancel_FB,
    ChargeAttackFinish_FB,

    ParryStart_FB, ParrySuccess_FB
}

/// <summary>
/// 플레이어의 피격 타입을 정의하는 열거형입니다.
/// </summary>
public enum PlayerDamagedType
{
    Normal = 0, // 일반 피격
    Strong = 1, // 강한 피격
    Defend = 2, // 방어 중 피격
    KnockDown = 3
}

/// <summary>
/// 플레이어의 모든 이벤트를 관리하고 피드백을 재생하는 클래스입니다.
/// </summary>
public class PlayerEvents : FeedbackPlayer<PlayerFeedbackType>
{
    [SerializeField] private Transform _rangedAttackStartPoint;

    #region Events
    public event Action<bool> OnBattleStateChaged; // 전투 상태 변경 이벤트

    public event Action OnDodgeFinish; // 회피 종료 이벤트

    public event Action OnAttackStart, OnAttackPerform; // 공격 시작, 공격 수행 이벤트
    public event Action<Collider> OnAttackAffect; // 공격 피격 이벤트
    public event Action OnAttackInputWindowStart, OnAttackFinish; // 공격 종료 이벤트

    public event Action<Collider> OnChargeAttackAffect; // 차지 공격 피격 이벤트

    public event Action OnParryStart, OnParryFinish;    
    public event Action OnParryWindowStart, OnParryWindowFinish; // 패링 수행 이벤트
    public event Action<Transform> OnParryDamageAffect; // 패링 성공 이벤트

    public event Action<bool> OnRegenStamina; // 스테미나 회복 이벤트
    #endregion

    #region EventHandler
    /// <summary>
    /// 전투 상태 변경 이벤트를 발생시킵니다.
    /// </summary>
    public void TriggerBattleStateChanged(bool isBattleState)
    {
        OnBattleStateChaged?.Invoke(isBattleState);
    }

    #region Movement
    /// <summary>
    /// 이동 중 발자국 피드백을 재생합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerMove()
    {
        PlayFeedback(PlayerFeedbackType.Move_FB, transform.position);
    }

    /// <summary>
    /// 이동 멈춤 피드백을 재생합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerMoveStop()
    {
        PlayFeedback(PlayerFeedbackType.MoveStop_FB, transform.position);
    }

    /// <summary>
    /// 회피 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerDodgeStart()
    {
        TriggerRegenStamina(false);
        PlayFeedback(PlayerFeedbackType.DodgeStart_FB, transform.position);
    }

    /// <summary>
    /// 회피 종료 이벤트를 발생시키고 피드백을 재생합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerDodgeFinish()
    {
        OnDodgeFinish?.Invoke();
        TriggerRegenStamina(true);
        PlayFeedback(PlayerFeedbackType.DodgeFinish_FB, transform.position);
    }

    /// <summary>
    /// 착지 피드백을 재생합니다.
    /// </summary>
    public void TriggerLanding()
    {
        PlayFeedback(PlayerFeedbackType.Landing_FB, transform.position);
    }
    #endregion

    #region Damaged
    /// <summary>
    /// 피격 타입에 맞는 피드백을 재생합니다.
    /// </summary>
    public void TriggerTakeDamge(PlayerDamagedType damagedType)
    {
        switch (damagedType)
        {
            case PlayerDamagedType.Normal: PlayFeedback(PlayerFeedbackType.TakeDamage_Normal_FB); break;
            case PlayerDamagedType.Strong: PlayFeedback(PlayerFeedbackType.TakeDamage_Strong_FB); break;
            case PlayerDamagedType.KnockDown: PlayFeedback(PlayerFeedbackType.TakeDamage_KnockDown_FB); break;
            case PlayerDamagedType.Defend: PlayFeedback(PlayerFeedbackType.TakeDamage_Defend_FB); break;
        }
    }
    #endregion

    /// <summary>
    /// 공격 수행 이벤트를 발생시킵니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerAttackPerform()
    {
        OnAttackPerform?.Invoke();
    }

    public void TriggerAttackInputWindowStart()
    {
        OnAttackInputWindowStart?.Invoke();
    }

    public void TriggerAttackFinish()
    {
        OnAttackFinish?.Invoke();
    }

    #region Attack
    /// <summary>
    /// 첫 번째 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerFirstAttackStart()
    {
        OnAttackStart?.Invoke();
        TriggerRegenStamina(false);
        PlayFeedback(PlayerFeedbackType.FirstAttackStart_FB);
    }

    /// <summary>
    /// 두 번째 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerSecondAttackStart()
    {
        OnAttackStart?.Invoke();
        TriggerRegenStamina(false);
        PlayFeedback(PlayerFeedbackType.SecondAttackStart_FB);
    }

    /// <summary>
    /// 세 번째 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerThirdAttackStart()
    {
        OnAttackStart?.Invoke();
        TriggerRegenStamina(false);
        PlayFeedback(PlayerFeedbackType.ThirdAttackStart_FB);
    }

    /// <summary>
    /// 근접 공격 타격 이벤트를 발생시키고 피드백을 재생합니다.
    /// </summary>
    public void TriggerAttackAffect(Collider collider)
    {
        OnAttackAffect?.Invoke(collider);
        PlayFeedback(PlayerFeedbackType.MeleeAttackHit_FB, collider.transform.position);
    }
    #endregion

    #region ChargeAttack
    /// <summary>
    /// 차지 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeStart()
    {
        OnAttackStart?.Invoke();
        TriggerRegenStamina(false);
        PlayFeedback(PlayerFeedbackType.ChargeStart_FB);
    }

    /// <summary>
    /// 차지 취소 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeCancel()
    {
        TriggerRegenStamina(true);
        StopFeedback(PlayerFeedbackType.ChargeStart_FB);
        PlayFeedback(PlayerFeedbackType.ChargeCancel_FB);
    }

    /// <summary>
    /// 차지 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeAttackStart(int tier)
    {
        TriggerRegenStamina(false);
        StopFeedback(PlayerFeedbackType.ChargeStart_FB);
        OnAttackStart?.Invoke();
    }

    /// <summary>
    /// 차지 공격 종료 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeAttackFinish()
    {
        TriggerRegenStamina(true);
        PlayFeedback(PlayerFeedbackType.ChargeAttackFinish_FB);
    }

    /// <summary>
    /// 차지 공격 피격 이벤트를 발생시키고 티어에 맞는 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeAttackAffect(Collider collider)
    {
        OnChargeAttackAffect?.Invoke(collider);
    }
    #endregion

    #region Parry

    /// <summary>
    /// 패링 시작 이벤트
    /// </summary>
    public void TriggerParryStart()
    {
        OnParryStart?.Invoke();
        PlayFeedback(PlayerFeedbackType.ParryStart_FB);
    }

    /// <summary>
    /// 패링 검사 시작 (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerParryWindowStart()
    {
        OnParryWindowStart?.Invoke();
    }

    /// <summary>
    /// 패링 성공 이벤트를 발생시키고 피드백을 재생합니다.
    /// </summary>
    public void TriggerParryDamageAffect(Transform transform)
    {
        OnParryDamageAffect?.Invoke(transform);
        PlayFeedback(PlayerFeedbackType.ParrySuccess_FB);
    }

    /// <summary>
    /// 패링 검사 종료 (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerParryWindowFinish()
    {
        OnParryWindowFinish?.Invoke();
    }    

    /// <summary>
    /// 패링 종료 시 호출 (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerParryFinish()
    {
        OnParryFinish?.Invoke();
    }

    #endregion

    public void TriggerRegenStamina(bool canRegen)
    {
        OnRegenStamina?.Invoke(canRegen);
    }

    #endregion
}