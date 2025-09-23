using BH_Lib.Log;
using MoreMountains.Feedbacks;
using refactor;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerFeedbackType
{
    Move_FB, MoveStop_FB, DodgeStart_FB,
    DodgeFinish_FB, Landing_FB,

    TakeDamage_Nomal_FB,
    TakeDamage_Strong_FB,
    TakeDamage_Defend_FB,

    FirstAttackStart_FB, SecondAttackStart_FB,
    ThirdAttackStart_FB, MeleeAttackHit_FB,

    ChargeStart_FB, ChargeCancel_FB,
    ChargeFinish_FB, ChargeAttackStart_FB,
    ChargeAttackFinish_FB, Tier1ChargeAttackHit_FB,
    Tier2ChargeAttackHit_FB, Tier3ChargeAttackHit_FB,

    RangeAttackChargeStart_FB, RangeAttackCharging_FB,
    RangeAttackChargeCancel_FB, RangeAttackChargeFinish_FB,
    RangeAttackStart_FB, RangeAttackHit_FB,

    ParryStart_FB, ParrySuccess_FB, CounterAttackStart_FB,
    Tier1CounterAttackFirstHit_FB, Tier2CounterAttackFirstHit_FB, Tier3CounterAttackFirstHit_FB,
    Tier1CounterAttackSecondHit_FB, Tier2CounterAttackSecondHit_FB, Tier3CounterAttackSecondHit_FB,
    CounterAttackFinish_FB,

    Tier1Up_FB, Tier2Up_FB, Tier3Up_FB,
    Tier1Down_FB, Tier2Down_FB, Tier3Down_FB,

    OverHeatStart_FB, OverHeatFinish_FB,

    Tier1_FB, Tier2_FB, Tier3_FB, OverHeat_FB
}

public class PlayerEvents : MonoBehaviour
{
    [SerializeField] private FeedbackPlayer<PlayerFeedbackType> _feedbackPlayer;

    public FeedbackPlayer<PlayerFeedbackType> Feedback => _feedbackPlayer;

    #region EffectPoint
    /// <summary>
    /// 근접 공격 효과 발생 위치
    /// </summary>
    [SerializeField] private Transform _firstAttackStartEffectPoint;
    [SerializeField] private Transform _secondAttackStartEffectPoint;
    [SerializeField] private Transform _thirdAttackStartEffectPoint;

    /// <summary>
    /// 차지 효과 발생 위치
    /// </summary>
    [SerializeField] private Transform _chargeEffectPoint;

    /// <summary>
    /// 원거리 공격 효과 발생 위치
    /// </summary>
    [SerializeField] private Transform _rangedAttackPoint;
    #endregion

    #region Events
    public event Action<bool> OnBattleStateChaged;

    public event Action OnDodgeFinish;

    public event Action OnAttackPerform;
    public event Action<Collider> OnAttackAffect;
    public event Action OnAttackFinish;

    public event Action<Collider> OnChargeAttackAffect;

    public event Action<Transform> OnRangedAttackStart;
    public event Action<Collider> OnRangedAttackAffect;
    public event Action OnRangedAttackFinish;

    public event Action OnParryPerform;
    public event Action<Collider> OnParryAffect;
    public event Action<Collider> OnFirstCounterAttackAffect;
    public event Action<Collider> OnSecondCounterAttackAffect;

    public event Action OnTier1Up, OnTier2Up, OnTier3Up, OnOverHeatStart;
    public event Action OnTier1Down, OnTier2Down, OnTier3Down, OnOverHeatFinish;
    #endregion

    private void Start()
    {
        _feedbackPlayer.Initialize();
    }

    #region EventHandler
    /// <summary>
    /// 전투 상태 변경 시 호출
    /// </summary>
    /// <param name="isBattleState">전투 상태 여부</param>
    public void TriggerBattleStateChanged(bool isBattleState)
    {
        OnBattleStateChaged.Invoke(isBattleState);
    }

    /// <summary>
    /// 이동 중 발자국 효과 재생
    /// </summary>
    public void TriggerFootstep()
    {
        _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Move_FB, transform.position);
    }

    /// <summary>
    /// 회피 완료 시 효과 재생
    /// </summary>
    public void TriggerDodgeFinish()
    {
        OnDodgeFinish.Invoke();
        _feedbackPlayer.PlayFeedback(PlayerFeedbackType.DodgeFinish_FB, transform.position);
    }

    /// <summary>
    /// 첫 번째 근접 공격 시작 시 효과 재생
    /// </summary>
    public void TriggerFirstAttackStart()
    {
        if (_firstAttackStartEffectPoint != null)
        {
            Log.PrintWarning("첫 번째 공격 효과 위치 설정");
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.FirstAttackStart_FB, _firstAttackStartEffectPoint.position);
        }
    }

    /// <summary>
    /// 두 번째 근접 공격 시작 시 효과 재생
    /// </summary>
    public void TriggerSecondAttackStart()
    {
        if (_secondAttackStartEffectPoint != null)
        {
            Log.PrintWarning("두 번째 공격 효과 위치 설정");
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.SecondAttackStart_FB, _secondAttackStartEffectPoint.position);
        }
    }

    /// <summary>
    /// 세 번째 근접 공격 시작 시 효과 재생
    /// </summary>
    public void TriggerThirdAttackStart()
    {
        if (_thirdAttackStartEffectPoint != null)
        {
            Log.PrintWarning("세 번째 공격 효과 위치 설정");
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.ThirdAttackStart_FB, _thirdAttackStartEffectPoint.position);
        }
    }

    /// <summary>
    /// 근접 공격 수행 시 효과 재생
    /// </summary>
    public void TriggerAttackPerform()
    {
        OnAttackPerform.Invoke();
    }

    /// <summary>
    /// 근접 공격 타격 시 효과 재생
    /// </summary>
    /// <param name="collider">타격 대상 콜라이더</param>
    public void TriggerAttackAffect(Collider collider)
    {
        OnAttackAffect.Invoke(collider);
        _feedbackPlayer.PlayFeedback(PlayerFeedbackType.MeleeAttackHit_FB, collider.transform.position);
    }

    /// <summary>
    /// 근접 공격 완료 시 효과 재생
    /// </summary>
    public void TriggerAttackFinish()
    {
        OnAttackFinish.Invoke();
    }

    /// <summary>
    /// 차지 시작 시 효과 재생
    /// </summary>
    public void TriggerChargeStart()
    {
        if (_chargeEffectPoint != null)
        {
            Log.PrintWarning("차지 효과 위치 설정");
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.ChargeStart_FB, _chargeEffectPoint.position);
        }
    }

    /// <summary>
    /// 차지 완료 시 효과 재생
    /// </summary>
    public void TriggerChargeFinish()
    {
        if (_chargeEffectPoint != null)
        {
            Log.PrintWarning("차지 효과 위치 설정");
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.ChargeFinish_FB, _chargeEffectPoint.position);
        }
    }

    /// <summary>
    /// 차지 공격 타격 시 효과 재생
    /// </summary>
    /// <param name="collider">타격 대상 콜라이더</param>
    /// <param name="tier">현재 티어</param>
    public void TriggerChargeAttackAffect(Collider collider, int tier)
    {
        OnChargeAttackAffect.Invoke(collider);

        switch (tier)
        {
            case 1:
                _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Tier1ChargeAttackHit_FB, collider.transform.position);
                break;
            case 2:
                _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Tier2ChargeAttackHit_FB, collider.transform.position);
                break;
            case 3:
                _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Tier3ChargeAttackHit_FB, collider.transform.position);
                break;
        }
    }

    /// <summary>
    /// 원거리 공격 차지 완료 시 효과 재생
    /// </summary>
    public void TriggerRangedChargeFinish()
    {
        if (_chargeEffectPoint != null)
        {
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.RangeAttackChargeFinish_FB, _chargeEffectPoint.position);
        }
    }

    /// <summary>
    /// 원거리 공격 시작 시 효과 재생
    /// </summary>
    public void TriggerRangedAttackStart()
    {
        OnRangedAttackStart.Invoke(_rangedAttackPoint);
    }

    /// <summary>
    /// 원거리 공격 타격 시 효과 재생
    /// </summary>
    /// <param name="collider">타격 대상 콜라이더</param>
    public void TriggerRangedAttackAffect(Collider collider)
    {
        OnRangedAttackAffect.Invoke(collider);

        if (collider != null)
        {
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.RangeAttackHit_FB, collider.transform.position);
        }
    }

    /// <summary>
    /// 원거리 공격 완료 시 효과 재생
    /// </summary>
    public void TriggerRangedAttackFinish()
    {
        OnRangedAttackFinish.Invoke();
    }

    /// <summary>
    /// 패리 시작 시 효과 재생
    /// </summary>
    public void TriggerParryPerform()
    {
        OnParryPerform?.Invoke();
    }

    /// <summary>
    /// 패리 성공 시 효과 재생
    /// </summary>
    /// <param name="collider">패리 대상 콜라이더</param>
    public void TriggerParryAffect(Collider collider)
    {
        OnParryAffect?.Invoke(collider);

        if (collider != null)
        {
            _feedbackPlayer.PlayFeedback(PlayerFeedbackType.ParrySuccess_FB, collider.transform.position);
        }
    }

    /// <summary>
    /// 첫 번째 카운터 공격 타격 시 효과 재생
    /// </summary>
    /// <param name="collider">타격 대상 콜라이더</param>
    /// <param name="tier">현재 티어</param>
    public void TriggerFirstCounterAttackAffect(Collider collider, int tier)
    {
        OnFirstCounterAttackAffect.Invoke(collider);

        switch (tier)
        {
            case 1:
                _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Tier1CounterAttackFirstHit_FB, collider.transform.position);
                break;
            case 2:
                _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Tier2CounterAttackFirstHit_FB, collider.transform.position);
                break;
            case 3:
                _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Tier3CounterAttackFirstHit_FB, collider.transform.position);
                break;
        }
    }

    /// <summary>
    /// 두 번째 카운터 공격 타격 시 효과 재생
    /// </summary>
    /// <param name="collider">타격 대상 콜라이더</param>
    /// <param name="tier">현재 티어</param>
    public void TriggerSecondCounterAttackAffect(Collider collider, int tier)
    {
        OnSecondCounterAttackAffect.Invoke(collider);

        switch (tier)
        {
            case 1:
                _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Tier1CounterAttackSecondHit_FB, collider.transform.position);
                break;
            case 2:
                _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Tier2CounterAttackSecondHit_FB, collider.transform.position);
                break;
            case 3:
                _feedbackPlayer.PlayFeedback(PlayerFeedbackType.Tier3CounterAttackSecondHit_FB, collider.transform.position);
                break;
        }
    }

    /// <summary>
    /// 티어 상승 시 효과 재생
    /// </summary>
    /// <param name="tier">현재 티어</param>
    public void TriggerTierUp(int tier)
    {
        switch (tier)
        {
            case 1:
                OnTier1Up?.Invoke();
                break;
            case 2:
                OnTier2Up?.Invoke();
                break;
            case 3:
                OnTier3Up?.Invoke();
                break;
            case 4:
                OnOverHeatStart?.Invoke();
                break;
        }
    }

    /// <summary>
    /// 티어 하락 시 효과 재생
    /// </summary>
    /// <param name="tier">현재 티어</param>
    public void TriggerTierDown(int tier)
    {
        switch (tier)
        {
            case 0:
                OnTier1Down?.Invoke();
                break;
            case 1:
                OnTier2Down?.Invoke();
                break;
            case 2:
                OnTier3Down?.Invoke();
                break;
            case 3:
                OnOverHeatFinish?.Invoke();
                break;
        }
    }
    #endregion
}