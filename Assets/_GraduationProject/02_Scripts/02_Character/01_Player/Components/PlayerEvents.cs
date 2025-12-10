using BH_Lib.AssetManager;
using BH_Lib.Log;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
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
        OnBattleStateChaged?.Invoke(isBattleState);
    }

    #region Movement

    /// <summary>
    /// 회피 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerDodgeStart()
    {
        TriggerRegenStamina(false);
    }

    /// <summary>
    /// 회피 종료 이벤트를 발생시키고 피드백을 재생합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerDodgeFinish()
    {
        OnDodgeFinish?.Invoke();
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
    public void TriggerTakeDamge(PlayerDamagedType damagedType)
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
    }

    /// <summary>
    /// 두 번째 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerSecondAttackStart()
    {
        OnAttackStart?.Invoke();
        TriggerRegenStamina(false);
    }

    /// <summary>
    /// 세 번째 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerThirdAttackStart()
    {
        OnAttackStart?.Invoke();
        TriggerRegenStamina(false);
    }

    /// <summary>
    /// 근접 공격 타격 이벤트를 발생시키고 피드백을 재생합니다.
    /// </summary>
    public void TriggerAttackAffect(Collider collider)
    {
        OnAttackAffect?.Invoke(collider);
    }
    #endregion

    #region ChargeAttack
    /// <summary>
    /// 차지 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeStart()
    {
        OnAttackStart?.Invoke();
        PlayFeedback("ChargeStart_FB");
        TriggerRegenStamina(false);
    }

    /// <summary>
    /// 차지 취소 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeCancel()
    {
        TriggerRegenStamina(true);
        StopFeedback("ChargeStart_FB");
        PlayFeedback("ChargeCancel_FB");
    }

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
    /// 차지 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeAttackStart()
    {
        TriggerRegenStamina(false);
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
        OnAttackStart?.Invoke();
    }

    /// <summary>
    /// 차지 공격 종료 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeAttackFinish()
    {
        TriggerRegenStamina(true);
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
        PlayFeedback("Parrying_Sucess_FB", transform.position);
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