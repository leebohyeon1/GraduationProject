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

    TakeDamage_Normal_FB, TakeDamage_Strong_FB, TakeDamage_Defend_FB,

    FirstAttackStart_FB, SecondAttackStart_FB,
    ThirdAttackStart_FB, MeleeAttackHit_FB,

    ChargeStart_FB, ChargeCancel_FB, ChargeFinish_FB, 
    Tier1ChargeAttackStart_FB, Tier2ChargeAttackStart_FB, Tier3ChargeAttackStart_FB,
    ChargeAttackFinish_FB,

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

/// <summary>
/// 플레이어의 공격 타입을 정의하는 열거형입니다.
/// </summary>
public enum PlayerAttackType
{
    Attack = 0, // 일반 공격
    ChargeAttack = 1, // 차지 공격
    CounterAttack = 2, // 카운터 공격
}

/// <summary>
/// 플레이어의 피격 타입을 정의하는 열거형입니다.
/// </summary>
public enum PlayerDamagedType
{
    Normal = 0, // 일반 피격
    Strong = 1, // 강한 피격
    Defend = 2 // 방어 중 피격
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
    public event Action OnAttackFinish; // 공격 종료 이벤트

    public event Action<Collider> OnChargeAttackAffect; // 차지 공격 피격 이벤트

    public event Action<Transform> OnRangedAttackStart; // 원거리 공격 시작 이벤트
    public event Action<Collider> OnRangedAttackAffect; // 원거리 공격 피격 이벤트
    public event Action OnRangedAttackFinish; // 원거리 공격 종료 이벤트

    public event Action OnParryPerform; // 패링 수행 이벤트
    public event Action<Collider> OnParryAffect; // 패링 성공 이벤트
    public event Action<Collider> OnFirstCounterAttackAffect; // 첫 번째 카운터 공격 피격 이벤트
    public event Action<Collider> OnSecondCounterAttackAffect; // 두 번째 카운터 공격 피격 이벤트

    public event Action OnTier1Up, OnTier2Up, OnTier3Up, OnOverHeatStart; // 티어 상승, 과열 시작 이벤트
    public event Action OnTier1Down, OnTier2Down, OnTier3Down, OnOverHeatFinish; // 티어 하락, 과열 종료 이벤트

    public event Action<int> OnOverHeat; // 과열 상태 이벤트

    public event Action<Vector2, float> OnFlashStart; // 점멸 스킬 이벤트
    public event Action<Vector3> OnFlashFinish;
    public event Action OnBoostStart; // 증폭 스킬 이벤트
    public event Action OnTimeStopStart; // 시간 정지 스킬 이벤트

    public event Action OnDataUpdate; // 데이터 업데이트 이벤트
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
        PlayFeedback(PlayerFeedbackType.DodgeStart_FB, transform.position);
    }

    /// <summary>
    /// 회피 종료 이벤트를 발생시키고 피드백을 재생합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerDodgeFinish()
    {
        OnDodgeFinish?.Invoke();
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
            case PlayerDamagedType.Normal: PlayFeedback(PlayerFeedbackType.TakeDamage_Normal_FB, transform.position); break;
            case PlayerDamagedType.Strong: PlayFeedback(PlayerFeedbackType.TakeDamage_Strong_FB, transform.position); break;
            case PlayerDamagedType.Defend: PlayFeedback(PlayerFeedbackType.TakeDamage_Defend_FB, transform.position); break;
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

    /// <summary>
    /// 공격 종료 이벤트를 발생시키고, 공격 타입에 맞는 종료 처리를 합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerAttackFinish(int type)
    {
        OnAttackFinish?.Invoke();

        switch((PlayerAttackType)type)
        {
            case PlayerAttackType.ChargeAttack: TriggerChargeAttackFinish(); break;
            case PlayerAttackType.CounterAttack: TriggerCounterAttackFinish(); break;
        }
    }

    #region Attack
    /// <summary>
    /// 첫 번째 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerFirstAttackStart()
    {
        OnAttackStart?.Invoke();
        PlayFeedback(PlayerFeedbackType.FirstAttackStart_FB);
    }

    /// <summary>
    /// 두 번째 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerSecondAttackStart()
    {
        OnAttackStart?.Invoke();
        PlayFeedback(PlayerFeedbackType.SecondAttackStart_FB);
    }

    /// <summary>
    /// 세 번째 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerThirdAttackStart()
    {
        OnAttackStart?.Invoke();
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
        PlayFeedback(PlayerFeedbackType.ChargeStart_FB);
    }

    /// <summary>
    /// 차지 취소 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeCancel()
    {
        StopFeedback(PlayerFeedbackType.ChargeStart_FB);
        PlayFeedback(PlayerFeedbackType.ChargeCancel_FB);
    }

    /// <summary>
    /// 차지 완료 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeFinish()
    {
        PlayFeedback(PlayerFeedbackType.ChargeFinish_FB);
    }

    /// <summary>
    /// 차지 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeAttackStart(int tier)
    {
        StopFeedback(PlayerFeedbackType.ChargeStart_FB);
        OnAttackStart?.Invoke();
        switch (tier)
        {
            case 1:
                PlayFeedback(PlayerFeedbackType.Tier1ChargeAttackStart_FB);
                break;
            case 2:
                PlayFeedback(PlayerFeedbackType.Tier2ChargeAttackStart_FB);
                break;
            case 3:
                PlayFeedback(PlayerFeedbackType.Tier3ChargeAttackStart_FB);
                break;
        }
    }

    /// <summary>
    /// 차지 공격 종료 피드백을 재생합니다.
    /// </summary>
    public void TriggerChargeAttackFinish()
    {
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

    #region RangedAttack
    /// <summary>
    /// 원거리 공격 차지 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerRangedChargeStart()
    {
        PlayFeedback(PlayerFeedbackType.RangeAttackChargeStart_FB );
    }

    /// <summary>
    /// 원거리 공격 차지 중 피드백을 재생합니다.
    /// </summary>
    public void TriggerRangedCharging()
    {
        PlayFeedback(PlayerFeedbackType.RangeAttackCharging_FB);
    }

    /// <summary>
    /// 원거리 공격 차지 취소 피드백을 재생합니다.
    /// </summary>
    public void TriggerRangedChargeCancel()
    {
        StopFeedback(PlayerFeedbackType.RangeAttackChargeStart_FB);
        PlayFeedback(PlayerFeedbackType.RangeAttackChargeCancel_FB);
    }

    /// <summary>
    /// 원거리 공격 차지 완료 피드백을 재생합니다.
    /// </summary>
    public void TriggerRangedChargeFinish()
    {
        PlayFeedback(PlayerFeedbackType.RangeAttackChargeFinish_FB);
    }

    /// <summary>
    /// 원거리 공격 시작 이벤트를 발생시키고 피드백을 재생합니다.
    /// </summary>
    public void TriggerRangedAttackStart()
    {
        OnRangedAttackStart?.Invoke(_rangedAttackStartPoint);
        PlayFeedback(PlayerFeedbackType.RangeAttackStart_FB, _rangedAttackStartPoint.position);
    }

    /// <summary>
    /// 원거리 공격 피격 이벤트를 발생시키고 피드백을 재생합니다.
    /// </summary>
    public void TriggerRangedAttackAffect(Collider collider)
    {
        OnRangedAttackAffect?.Invoke(collider);
        if (collider != null)
        {
            PlayFeedback(PlayerFeedbackType.RangeAttackHit_FB, collider.transform.position);
        }
    }

    /// <summary>
    /// 원거리 공격 종료 이벤트를 발생시킵니다.
    /// </summary>
    public void TriggerRangedAttackFinish()
    {
        OnRangedAttackFinish?.Invoke();
    }
    #endregion

    #region Parry
    /// <summary>
    /// 패링 수행 이벤트를 발생시키고 피드백을 재생합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void TriggerParryPerform()
    {
        OnParryPerform?.Invoke();
        PlayFeedback(PlayerFeedbackType.ParryStart_FB);
    }

    /// <summary>
    /// 패링 성공 이벤트를 발생시키고 피드백을 재생합니다.
    /// </summary>
    public void TriggerParryAffect(Collider collider)
    {
        OnParryAffect?.Invoke(collider);
        //PlayFeedback(PlayerFeedbackType.ParrySuccess_FB);
    }
    #endregion

    #region CounterAttack
    /// <summary>
    /// 첫 번째 카운터 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerFirstCounterAttackStart()
    {
        PlayFeedback(PlayerFeedbackType.CounterFirstAttackStart_FB);
    }

    /// <summary>
    /// 첫 번째 카운터 공격 피격 이벤트를 발생시키고 티어에 맞는 피드백을 재생합니다.
    /// </summary>
    public void TriggerFirstCounterAttackAffect(Collider collider, int tier)
    {
        OnFirstCounterAttackAffect?.Invoke(collider);

        switch (tier)
        {
            case 1: PlayFeedback(PlayerFeedbackType.Tier1CounterAttackFirstHit_FB, collider.transform.position); break;
            case 2: PlayFeedback(PlayerFeedbackType.Tier2CounterAttackFirstHit_FB, collider.transform.position); break;
            case 3: PlayFeedback(PlayerFeedbackType.Tier3CounterAttackFirstHit_FB, collider.transform.position); break;
        }
    }

    /// <summary>
    /// 두 번째 카운터 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerSecondCounterAttackStart()
    {
        PlayFeedback(PlayerFeedbackType.CounterSecondAttackStart_FB);
    }

    /// <summary>
    /// 두 번째 카운터 공격 피격 이벤트를 발생시키고 티어에 맞는 피드백을 재생합니다.
    /// </summary>
    public void TriggerSecondCounterAttackAffect(Collider collider, int tier)
    {
        OnSecondCounterAttackAffect?.Invoke(collider);

        switch (tier)
        {
            case 1: PlayFeedback(PlayerFeedbackType.Tier1CounterAttackSecondHit_FB, collider.transform.position); break;
            case 2: PlayFeedback(PlayerFeedbackType.Tier2CounterAttackSecondHit_FB, collider.transform.position); break;
            case 3: PlayFeedback(PlayerFeedbackType.Tier3CounterAttackSecondHit_FB, collider.transform.position); break;
        }
    }
    
    /// <summary>
    /// 카운터 공격 종료 피드백을 재생합니다.
    /// </summary>
    public void TriggerCounterAttackFinish()
    {
        PlayFeedback(PlayerFeedbackType.CounterAttackFinish_FB);
    }
    #endregion

    #region Heat
    /// <summary>
    /// 티어 상승 시 티어별 피드백을 재생합니다.
    /// </summary>
    public void TriggerTierUp(int previousTier, int currentTier)
    {
        OnDataUpdate?.Invoke();

        for (int i = previousTier + 1; i <= currentTier; i++)
        {
            switch (i)
            {
                case 1: OnTier1Up?.Invoke(); PlayFeedback(PlayerFeedbackType.Tier1Up_FB); break;
                case 2: OnTier2Up?.Invoke(); PlayFeedback(PlayerFeedbackType.Tier2Up_FB); break;
                case 3: OnTier3Up?.Invoke(); PlayFeedback(PlayerFeedbackType.Tier3Up_FB); break;
                case 4: OnOverHeatStart?.Invoke(); PlayFeedback(PlayerFeedbackType.OverHeatStart_FB); break;
            }
        }
    }

    /// <summary>
    /// 티어 하락 시 티어별 피드백을 재생합니다.
    /// </summary>
    public void TriggerTierDown(int previousTier, int currentTier)
    {
        OnDataUpdate?.Invoke();

        for (int i = previousTier; i > currentTier; i--)
        {
            switch (i)
            {
                case 1: OnTier1Down?.Invoke(); PlayFeedback(PlayerFeedbackType.Tier1Down_FB); break;
                case 2: OnTier2Down?.Invoke(); PlayFeedback(PlayerFeedbackType.Tier2Down_FB); break;
                case 3: OnTier3Down?.Invoke(); PlayFeedback(PlayerFeedbackType.Tier3Down_FB); break;
            }
        }
    }

    /// <summary>
    /// 현재 티어에 맞는 지속 효과 피드백을 재생하고, 과열 상태일 경우 이벤트를 발생시킵니다.
    /// </summary>
    public void TriggerTier(int tier, int overHeatDamage)
    {
        switch (tier)
        {
            case 1: PlayFeedback(PlayerFeedbackType.Tier1_FB); break;
            case 2: PlayFeedback(PlayerFeedbackType.Tier2_FB); break;
            case 3: PlayFeedback(PlayerFeedbackType.Tier3_FB); break;
            case 4: 
                PlayFeedback(PlayerFeedbackType.OverHeat_FB);
                OnOverHeat?.Invoke(overHeatDamage);
                break;
        }
    }

    /// <summary>
    /// 오버히트 종료 피드백을 재생합니다.
    /// </summary>
    public void TriggerOverHeatFinish()
    {
        OnOverHeatFinish?.Invoke();
        PlayFeedback(PlayerFeedbackType.OverHeatFinish_FB);
    }
    #endregion

    #region Skill
    /// <summary>
    /// 점멸 스킬 시작 피드백을 재생합니다.
    /// </summary>
    /// <param name="input">입력</param>
    public void TriggerFlashSkillStart(Vector2 input, float distance)
    {
        OnFlashStart?.Invoke(input, distance);
    }
    /// <summary>
    /// 점멸 스킬 종료 피드백을 재생합니다.
    /// </summary>
    /// <param name="endPosition">종료 위치</param>
    public void TriggerFlashSkillFinish(Vector3 endPosition)
    {
        OnFlashFinish?.Invoke(endPosition);
    }
    /// <summary>
    /// 증폭 스킬 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerBoostSkillStart()
    {
        OnBoostStart?.Invoke(); 
    }

    #endregion

    #endregion
}