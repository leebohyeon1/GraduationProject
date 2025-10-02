using BH_Lib.AssetManager;
using BH_Lib.Log;
using MoreMountains.Feedbacks;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerFeedbackType
{
    Move_FB, MoveStop_FB, DodgeStart_FB,
    DodgeFinish_FB, Landing_FB,

    TakeDamage_Normal_FB,
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

    ParryStart_FB, ParrySuccess_FB, CounterFirstAttackStart_FB, CounterSecondAttackStart_FB,
    Tier1CounterAttackFirstHit_FB, Tier2CounterAttackFirstHit_FB, Tier3CounterAttackFirstHit_FB,
    Tier1CounterAttackSecondHit_FB, Tier2CounterAttackSecondHit_FB, Tier3CounterAttackSecondHit_FB,
    CounterAttackFinish_FB,

    Tier1Up_FB, Tier2Up_FB, Tier3Up_FB,
    Tier1Down_FB, Tier2Down_FB, Tier3Down_FB,

    OverHeatStart_FB, OverHeatFinish_FB,

    Tier1_FB, Tier2_FB, Tier3_FB, OverHeat_FB
}

public enum PlayerAttackType
{
    Attack = 0,
    ChargeAttack = 1,
    CounterAttack = 2,
}

public enum PlayerDamagedType
{
    Normal = 0,
    Strong = 1,
    Defend = 2
}

public class PlayerEvents : FeedbackPlayer<PlayerFeedbackType>
{
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
    [SerializeField] private Transform _chargeAttackPoint;

    /// <summary>
    /// 원거리 공격 효과 발생 위치
    /// </summary>
    [SerializeField] private Transform _rangedAttackPoint;

    /// <summary>
    /// 카운터 공격 효과 발생 위치
    /// </summary>
    [SerializeField] private Transform _counterAttackPoint;

    #endregion

    #region Events
    public event Action<bool> OnBattleStateChaged;

    public event Action OnDodgeFinish;

    public event Action OnAttackStart, OnAttackPerform;
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

    public event Action OnOverHeat;
    #endregion

    #region EventHandler
    /// <summary>
    /// 전투 상태 변경 시 호출
    /// </summary>
    /// <param name="isBattleState">전투 상태 여부</param>
    public void TriggerBattleStateChanged(bool isBattleState)
    {
        OnBattleStateChaged.Invoke(isBattleState);
    }

    #region Movement
    /// <summary>
    /// 이동 중 발자국 효과 재생
    /// (애니메이션 트리거)
    /// </summary>
    public void TriggerMove()
    {
        PlayFeedback(PlayerFeedbackType.Move_FB, transform.position);
    }

    /// <summary>
    /// 이동 멈춤 효과 재생
    /// (애니메이션 트리거)
    /// </summary>
    public void TriggerMoveStop()
    {
        PlayFeedback(PlayerFeedbackType.MoveStop_FB, transform.position);
    }

    /// <summary>
    /// 회피 시작 시 효과 재생
    /// </summary>
    public void TriggerDodgeStart()
    {
        PlayFeedback(PlayerFeedbackType.DodgeStart_FB, transform.position);
    }

    /// <summary>
    /// Todo: 회피가 순간이동으로 바뀌면서 이것도 바뀔 듯
    /// 회피 완료 시 효과 재생
    /// (애니메이션 트리거)
    /// </summary>
    public void TriggerDodgeFinish()
    {
        OnDodgeFinish.Invoke();
        PlayFeedback(PlayerFeedbackType.DodgeFinish_FB, transform.position);
    }

    /// <summary>
    /// 착지 했을 때 효과 재생
    /// </summary>
    public void TriggerLanding()
    {
        PlayFeedback(PlayerFeedbackType.Landing_FB, transform.position);
    }
    #endregion

    #region Damaged

    /// <summary>
    /// 피격 효과 재생
    /// </summary>
    /// <param name="damagedType">피격 효과</param>
    public void TriggerTakeDamge(PlayerDamagedType damagedType)
    {
        switch (damagedType)
        {
            case PlayerDamagedType.Normal:
                PlayFeedback(PlayerFeedbackType.TakeDamage_Normal_FB, transform.position);
                break;
            case PlayerDamagedType.Strong:
                PlayFeedback(PlayerFeedbackType.TakeDamage_Strong_FB, transform.position);
                break;
            case PlayerDamagedType.Defend:
                PlayFeedback(PlayerFeedbackType.TakeDamage_Defend_FB, transform.position);
                break;
        }
    }

    #endregion

    /// <summary>
    /// 근접 공격 수행 시 효과 재생
    /// (애니메이션 트리거)
    /// </summary>
    public void TriggerAttackPerform()
    {
        OnAttackPerform.Invoke();
    }

    /// <summary>
    /// 근접 공격 완료 시 효과 재생
    /// (애니메이션 트리거)
    /// </summary>
    public void TriggerAttackFinish(int type)
    {
        OnAttackFinish.Invoke();

        switch(type)
        {
            case (int)PlayerAttackType.Attack:

                break;
            case (int)PlayerAttackType.ChargeAttack:
                TriggerChargeAttackFinish();
                break;
            case (int)PlayerAttackType.CounterAttack:
                TriggerCounterAttackFinish();
                break;
        }    

    }

    #region Attack
    /// <summary>
    /// 첫 번째 근접 공격 시작 시 효과 재생
    /// </summary>
    public void TriggerFirstAttackStart()
    {
        if (_firstAttackStartEffectPoint != null)
        {
            OnAttackStart?.Invoke();
            Log.PrintWarning("첫 번째 공격 효과 위치 설정");
            PlayFeedback(PlayerFeedbackType.FirstAttackStart_FB, _firstAttackStartEffectPoint.position);
        }
    }

    /// <summary>
    /// 두 번째 근접 공격 시작 시 효과 재생
    /// </summary>
    public void TriggerSecondAttackStart()
    {
        if (_secondAttackStartEffectPoint != null)
        {
            OnAttackStart?.Invoke();
            Log.PrintWarning("두 번째 공격 효과 위치 설정");
            PlayFeedback(PlayerFeedbackType.SecondAttackStart_FB, _secondAttackStartEffectPoint.position);
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
            PlayFeedback(PlayerFeedbackType.ThirdAttackStart_FB, _thirdAttackStartEffectPoint.position);
        }
    }

    /// <summary>
    /// 근접 공격 타격 시 효과 재생
    /// </summary>
    /// <param name="collider">타격 대상 콜라이더</param>
    public void TriggerAttackAffect(Collider collider)
    {
        OnAttackAffect.Invoke(collider);
        PlayFeedback(PlayerFeedbackType.MeleeAttackHit_FB, collider.transform.position);
    }
    #endregion

    #region ChargeAttack
    /// <summary>
    /// 차지 시작 시 효과 재생
    /// </summary>
    public void TriggerChargeStart()
    {
        if (_chargeEffectPoint != null)
        {
            OnAttackStart?.Invoke();
            Log.PrintWarning("차지 효과 위치 설정");
            PlayFeedback(PlayerFeedbackType.ChargeStart_FB, _chargeEffectPoint.position);
        }
    }

    /// <summary>
    /// 차지 취소 되었을 때 효과 재생
    /// </summary>
    public void TriggerChargeCancel()
    {
        PlayFeedback(PlayerFeedbackType.ChargeCancel_FB, _chargeEffectPoint.position);
    }

    /// <summary>
    /// 차지 완료 시 효과 재생
    /// </summary>
    public void TriggerChargeFinish()
    {
        if (_chargeEffectPoint != null)
        {
            Log.PrintWarning("차지 효과 위치 설정");
            PlayFeedback(PlayerFeedbackType.ChargeFinish_FB, _chargeEffectPoint.position);
        }
    }

    /// <summary>
    /// 차지 공격 시작 시 호출
    /// </summary>
    public void TriggerChargeAttackStart()
    {
        OnAttackStart?.Invoke();
        PlayFeedback(PlayerFeedbackType.ChargeAttackStart_FB, _chargeAttackPoint.position);
    }    

    /// <summary>
    /// 차지 공격 종료 시 호출
    /// </summary>
    public void TriggerChargeAttackFinish()
    {
        PlayFeedback(PlayerFeedbackType.ChargeAttackFinish_FB, _chargeAttackPoint.position);
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
                PlayFeedback(PlayerFeedbackType.Tier1ChargeAttackHit_FB, collider.transform.position);
                break;
            case 2:
                PlayFeedback(PlayerFeedbackType.Tier2ChargeAttackHit_FB, collider.transform.position);
                break;
            case 3:
                PlayFeedback(PlayerFeedbackType.Tier3ChargeAttackHit_FB, collider.transform.position);
                break;
        }
    }
    #endregion

    #region RangedAttack

    /// <summary>
    /// 원거리 공격 차지 시작 시 효과 재생
    /// </summary>
    public void TriggerRangedChargeStart()
    {
        if (_chargeEffectPoint != null)
        {
            PlayFeedback(PlayerFeedbackType.RangeAttackChargeStart_FB, _chargeEffectPoint.position);
        }
    }

    /// <summary>
    /// 원거리 공격 차지 효과 재생
    /// </summary>
    public void TriggerRangedCharging()
    {
        PlayFeedback(PlayerFeedbackType.RangeAttackCharging_FB, _chargeEffectPoint.position);
    }

    /// <summary>
    /// 원거리 공격 차지 취소 시 효과 재생
    /// </summary>
    public void TriggerRangedChargeCancel()
    {
        if (_chargeEffectPoint != null)
        {
            PlayFeedback(PlayerFeedbackType.RangeAttackChargeCancel_FB, _chargeEffectPoint.position);
        }
    }

    /// <summary>
    /// 원거리 공격 차지 완료 시 효과 재생
    /// </summary>
    public void TriggerRangedChargeFinish()
    {
        if (_chargeEffectPoint != null)
        {
            PlayFeedback(PlayerFeedbackType.RangeAttackChargeFinish_FB, _chargeEffectPoint.position);
        }
    }

    /// <summary>
    /// 원거리 공격 시작 시 효과 재생
    /// </summary>
    public void TriggerRangedAttackStart()
    {
        OnRangedAttackStart.Invoke(_rangedAttackPoint);

        PlayFeedback(PlayerFeedbackType.RangeAttackStart_FB, _rangedAttackPoint.position);
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
            PlayFeedback(PlayerFeedbackType.RangeAttackHit_FB, collider.transform.position);
        }
    }

    /// <summary>
    /// 원거리 공격 완료 시 효과 재생
    /// </summary>
    public void TriggerRangedAttackFinish()
    {
        OnRangedAttackFinish.Invoke();
    }
    #endregion

    #region Parry

    /// <summary>
    /// 패리 시작 시 효과 재생
    /// (애니메이션 트리거)
    /// </summary>
    public void TriggerParryPerform()
    {
        OnParryPerform?.Invoke();
        PlayFeedback(PlayerFeedbackType.ParryStart_FB, transform.position);
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
            PlayFeedback(PlayerFeedbackType.ParrySuccess_FB, collider.transform.position);
        }
    }
    #endregion

    #region CounterAttack

    /// <summary>
    /// 첫번째 카운터 공격 시작 시 효과 재생
    /// </summary>
    public void TriggerFirstCounterAttackStart()
    {
        PlayFeedback(PlayerFeedbackType.CounterFirstAttackStart_FB, _counterAttackPoint.position);
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
                PlayFeedback(PlayerFeedbackType.Tier1CounterAttackFirstHit_FB, collider.transform.position);
                break;
            case 2:
                PlayFeedback(PlayerFeedbackType.Tier2CounterAttackFirstHit_FB, collider.transform.position);
                break;
            case 3:
                PlayFeedback(PlayerFeedbackType.Tier3CounterAttackFirstHit_FB, collider.transform.position);
                break;
        }
    }

    /// <summary>
    /// 두번째 카운터 공격 시작 시 효과 재생
    /// </summary>
    public void TriggerSecondCounterAttackStart()
    {
        PlayFeedback(PlayerFeedbackType.CounterSecondAttackStart_FB, _counterAttackPoint.position);
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
                PlayFeedback(PlayerFeedbackType.Tier1CounterAttackSecondHit_FB, collider.transform.position);
                break;
            case 2:
                PlayFeedback(PlayerFeedbackType.Tier2CounterAttackSecondHit_FB, collider.transform.position);
                break;
            case 3:
                PlayFeedback(PlayerFeedbackType.Tier3CounterAttackSecondHit_FB, collider.transform.position);
                break;
        }
    }
    
    /// <summary>
    /// 카운터 공격 종료 시 효과 재생
    /// </summary>
    public void TriggerCounterAttackFinish()
    {
        PlayFeedback(PlayerFeedbackType.CounterAttackFinish_FB, _counterAttackPoint.position);
    }
    #endregion

    #region Heat
    /// <summary>
    /// 티어 상승 시 효과 재생
    /// </summary>
    /// <param name="previousTier">이전 티어</param>
    /// <param name="currentTier">현재 티어</param>
    public void TriggerTierUp(int previousTier, int currentTier)
    {
        for(int i = previousTier + 1; i <= currentTier; i++)
        {
            switch (i)
            {
                case 1:
                    OnTier1Up?.Invoke();
                    PlayFeedback(PlayerFeedbackType.Tier1Up_FB, transform.position);
                    break;
                case 2:
                    OnTier2Up?.Invoke();
                    PlayFeedback(PlayerFeedbackType.Tier2Up_FB, transform.position);
                    break;
                case 3:
                    OnTier3Up?.Invoke();
                    PlayFeedback(PlayerFeedbackType.Tier3Up_FB, transform.position);
                    break;
                case 4:
                    OnOverHeatStart?.Invoke();
                    PlayFeedback(PlayerFeedbackType.OverHeatStart_FB, transform.position);
                    break;
            }
        }
    }

    /// <summary>
    /// 티어 하락 시 효과 재생
    /// </summary>
    /// <param name="previousTier">이전 티어</param>
    /// <param name="currentTier">현재 티어</param>
    public void TriggerTierDown(int previousTier, int currentTier)
    {
        for (int i = previousTier; i > currentTier; i--)
        {
            // 반복문의 현재 값 'i'는 우리가 거쳐 내려오는 각 티어를 의미합니다.
            switch (i)
            {
                // Tier 1에서 0으로 내려올 때
                case 1:
                    OnTier1Down?.Invoke();
                    PlayFeedback(PlayerFeedbackType.Tier1Down_FB, transform.position);
                    break;
                // Tier 2에서 1로 내려올 때
                case 2:
                    OnTier2Down?.Invoke();
                    PlayFeedback(PlayerFeedbackType.Tier2Down_FB, transform.position);
                    break;
                // Tier 3에서 2로 내려올 때
                case 3:
                    OnTier3Down?.Invoke();
                    PlayFeedback(PlayerFeedbackType.Tier3Down_FB, transform.position);
                    break;
                // OverHeat(Tier 4로 가정)에서 3으로 내려올 때
                case 4:
                    OnOverHeatFinish?.Invoke();
                    PlayFeedback(PlayerFeedbackType.OverHeatFinish_FB, transform.position);
                    break;
            }
        }
    }

    /// <summary>
    /// 티어 효과 재생
    /// </summary>
    /// <param name="tier">현재 티어</param>
    public void TriggerTier(int tier)
    {
        switch (tier)
        {
            case 1:
                PlayFeedback(PlayerFeedbackType.Tier1_FB, transform.position);
                break;
            case 2:
                PlayFeedback(PlayerFeedbackType.Tier2_FB, transform.position);
                break;
            case 3:
                PlayFeedback(PlayerFeedbackType.Tier3_FB, transform.position);
                break;
            case 4:
                PlayFeedback(PlayerFeedbackType.OverHeat_FB, transform.position);
                OnOverHeat.Invoke();
                break;
        }
    }

    #endregion

    #endregion
}