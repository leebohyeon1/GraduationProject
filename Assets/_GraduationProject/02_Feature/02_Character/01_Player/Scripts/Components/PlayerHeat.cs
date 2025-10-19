using BH_Lib.Log;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// 플레이어의 열기 시스템을 관리하는 컴포넌트입니다.
/// </summary>
public class PlayerHeat : MonoBehaviour, IHeatable ,  IDisposable
{
    private PlayerStats _stats; // 플레이어 스탯
    private SourceMapDatabaseSO _sourceMapDataBase; // 소스맵 데이터
    private TierStatDatabaseSO _tierStatDataBase;   // 티어 데이터
    private OverHeatDataSO _overHeatData; // 과열 데이터
    private PlayerEvents _events; // 플레이어 이벤트


    private float _heatTierTimer; // 열기 티어 타이머
    private ActorType _actorType; // 액터 타입

    public bool IsOverHeat => _stats.IsOverHeat; // 과열 상태 여부
    public bool IsHeatLock => _stats.IsHeatlock; // 열기 변경 잠금 여부
    public ActorType ActorType => _actorType;

    public int MaxHeat => 100;  // 최대 열기
    public int CurrentHeat => _stats.CurrentHeat; // 현재 열기
    public int CurrentTier => GetTier();    // 현재 티어


    private Sequence _overheatSequence; // 과열 시퀀스
    private Sequence _battleOutSequence; // 전투 종료 시퀀스

    public event Action<int, int> OnHeatChanged;    // 열기 변경 이벤트
    public event Action<int, int> OnTierChanged;    // 티어 변경 이벤트

    public event Action<float> OnChargeGuageChanged;

    /// <summary>
    /// 열기 시스템을 초기화합니다.
    /// </summary>
    public void Initialize( PlayerStats playerStats, SourceMapDatabaseSO sourceMapDatabaseSO, TierStatDatabaseSO tierStatDatabaseSO, OverHeatDataSO overHeatDataSO, PlayerEvents events)
    {
        _stats = playerStats;
        _sourceMapDataBase = sourceMapDatabaseSO;
        _tierStatDataBase = tierStatDatabaseSO;
        _overHeatData = overHeatDataSO;
        _events = events;

        _actorType = ActorType.Player;

        // 이벤트 구독
        _events.OnBattleStateChaged += HandleBattleSateChanged;
        OnTierChanged += HandleHeatChanged;
        _events.OnAttackAffect += HandleAttackAffect;
        _events.OnChargeAttackAffect += HandleChargeAttackAffect;
        _events.OnParryAffect += HandleParryAffect;
        _events.OnRangedAttackAffect += HandleRangedAttackAffect;
        _events.OnSecondCounterAttackAffect += HandleSecondCounterAttackAffect;
        _events.OnOverHeatFinish += HandleOverHeatFinish;
        _events.OnBoostStart += HandleBoostStart;
    } 

    /// <summary>
    /// 리소스 해제 함수
    /// </summary>
    public void Dispose()
    {
        // 이벤트 구독 해제
        _events.OnBattleStateChaged -= HandleBattleSateChanged;
        OnTierChanged -= HandleHeatChanged;
        _events.OnAttackAffect -= HandleAttackAffect;
        _events.OnChargeAttackAffect -= HandleChargeAttackAffect;
        _events.OnParryAffect -= HandleParryAffect;
        _events.OnRangedAttackAffect -= HandleRangedAttackAffect;
        _events.OnSecondCounterAttackAffect -= HandleSecondCounterAttackAffect;
        _events.OnOverHeatFinish -= HandleOverHeatFinish;
        _events.OnBoostStart -= HandleBoostStart;
    }

    #region SetHeat
    /// <summary>
    /// 열기를 변경합니다.
    /// </summary>
    /// <param name="amount"> 열기 변화량 </param>
    public void ChangeHeat(int amount)
    {
        if ( amount == 0 && IsHeatLock) return;

        int previousTier = GetTier();
        int previousHeat = CurrentHeat;

        _stats.CurrentHeat = Mathf.Clamp(CurrentHeat + amount, 0, MaxHeat);

        if (previousHeat != CurrentHeat)
        {
            OnHeatChanged?.Invoke(previousHeat, CurrentHeat);
        }

        int newTier = GetTier();
        if (previousTier != newTier)
        {
            OnTierChanged?.Invoke(previousTier, CurrentTier);
        }

        if(CurrentHeat >= MaxHeat && !IsOverHeat)
        {
            OverHeat();
        }
    }

    /// <summary>
    /// 열기를 설정합니다.
    /// </summary>
    /// <param name="amount"> 열기 설정값 </param>
    public void SetHeat(int amount)
    {
        if (IsHeatLock) return;

        int previousTier = GetTier();
        int previousHeat = CurrentHeat;

        _stats.CurrentHeat = Mathf.Clamp(amount, 0, MaxHeat);

        if (previousHeat != CurrentHeat)
        {
            OnHeatChanged?.Invoke(previousHeat, CurrentHeat);
        }

        int newTier = GetTier();
        if (previousTier != newTier)
        {
            OnTierChanged?.Invoke(previousTier, CurrentTier);
        }

        if (CurrentHeat >= MaxHeat && !IsOverHeat)
        {
            OverHeat();
        }
    }
    #endregion

    #region OverHeat
    /// <summary>
    /// 과열 상태로 전환합니다.
    /// </summary>
    private void OverHeat()
    {
        if (_overheatSequence != null && _overheatSequence.IsActive()) return;

        _overheatSequence = DOTween.Sequence();
        _overheatSequence.SetDelay(_overHeatData.DelaySecond)
            .AppendCallback(() =>
            {
                SetHeatLock(true);
                _heatTierTimer = Time.time;
                _stats.IsOverHeat = true;
            });
    }

    /// <summary>
    /// 열기 티어 효과를 적용할 수 있는지 확인합니다.
    /// </summary>
    public bool CanHeatTierEffect()
    {
        _heatTierTimer += Time.deltaTime;

        if (_heatTierTimer > _overHeatData.TickSecond)
        {
            _heatTierTimer = 0f;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 과열 상태를 종료합니다.
    /// </summary>
    public void OverHeatFinish()
    {
        _overheatSequence?.Kill();

        if (_stats.IsOverHeat)
        {
            _stats.IsOverHeat = false;
            SetHeatLock(false);
        }
    }
    #endregion

    /// <summary>
    /// 근접 공격 시 대상의 열기를 증가시킵니다.
    /// </summary>
    public void IncreaseHeatOnAttack(Collider collider)
    {
        if (collider.TryGetComponent<IHeatable>(out var heatable))
        {
            SourceMap sourceMap = _sourceMapDataBase.GetSourceMap("OnMeleeHit", heatable.ActorType, -1);
            int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
            heatable.ChangeHeat(deltaHeat);
        }
    }

    /// <summary>
    /// 차지 게이지에 따라 자신의 열기를 증가시킵니다.
    /// </summary>
    public void IncreaseHeatOnCharge(SourceMap sourceMap, float chargeGuage)
    {
        OnChargeGuageChanged?.Invoke(chargeGuage);
        if (chargeGuage >= CurrentHeat)
        {
            SetHeat(Mathf.FloorToInt(chargeGuage));
        }
    }

    /// <summary>
    /// 차지 공격 시 대상의 열기를 증가시키고 자신의 열기를 초기화합니다.
    /// </summary>
    public void IncreaseHeatOnChargeAttack(Collider collider)
    {
        if (collider.TryGetComponent<IHeatable>(out var heatable))
        {
            SourceMap sourceMap = _sourceMapDataBase.GetSourceMap("OnChargeAttack", heatable.ActorType, CurrentTier);
            int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
            heatable.ChangeHeat(deltaHeat);
        }
    }

    /// <summary>
    /// 원거리 공격 시 대상의 열기를 감소시킵니다.
    /// </summary>
    public void DecreaseHeatOnRangeAttack(Collider collider)
    {
        if (collider.TryGetComponent<IHeatable>(out var heatable))
        {
            SourceMap sourceMap = _sourceMapDataBase.GetSourceMap("OnIceBallSuccess", heatable.ActorType, -1);
            int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
            heatable.ChangeHeat(deltaHeat);
        }
    }

    /// <summary>
    /// 패리 성공 시 자신의 열기를 증가시킵니다.
    /// </summary>
    public void IncreaseHeatOnParrySuccess()
    {
        SourceMap sourceMap = _sourceMapDataBase.GetSourceMap("OnParrySuccess", -1);
        int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
        ChangeHeat(deltaHeat);
    }

    /// <summary>
    /// 전투 종료 시 자신의 열기를 감소시킵니다.
    /// </summary>
    public void DecreaseHeatOnBattleOut()
    {
        if(_stats.SkillData.IsMaxLevelBoost && CurrentHeat <= 0) return;

        SourceMap sourceMap = _sourceMapDataBase.GetSourceMap("OnBattleOut", -1);
        int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
        ChangeHeat(deltaHeat);
    }

    /// <summary>
    /// 카운터 공격 시 대상의 열기를 증가시키고 자신의 열기를 초기화합니다.
    /// </summary>
    public void IncreaseHeatOnCounterAttack(Collider collider)
    {
        if(collider.TryGetComponent<IHeatable>(out var heatable))
        {
            SourceMap sourceMap = _sourceMapDataBase.GetSourceMap("OnCounterSuccess", heatable.ActorType, CurrentTier);
            int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
            heatable.ChangeHeat(deltaHeat);
        }
        SetHeat(0);
    }

    /// <summary>
    /// 열기 변경 잠금 여부 설정한다.
    /// </summary>
    /// <param name="isLock">잠금 여부</param>
    public void SetHeatLock(bool isLock)
    {
        _stats.IsHeatlock = isLock;
    }

    /// <summary>
    /// 현재 티어 반환한다.
    /// </summary>
    /// <returns>현재 티어</returns>
    public int GetTier()
    {
        return _tierStatDataBase.GetCurrentTier(CurrentHeat);
    }

    #region Event
    public void TriggerChargeGuageChanged(float guage)
    {
        OnChargeGuageChanged?.Invoke(guage);
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// 전투 상태 변경 시 호출됩니다.
    /// </summary>
    private void HandleBattleSateChanged(bool isBattleState)
    {
        _battleOutSequence?.Kill();

        if(!isBattleState)
        {
            SetHeatLock(false);

            _battleOutSequence = DOTween.Sequence()
                .AppendCallback(DecreaseHeatOnBattleOut)
                .SetDelay(1f)
                .SetLoops(-1, LoopType.Restart);

            _battleOutSequence.Play();
        }
    }

    /// <summary>
    /// 열기 티어 변경 시 호출됩니다.
    /// </summary>
    private void HandleHeatChanged(int previousTier, int currentTier)
    {
        if (currentTier > previousTier)
        {
            _events.TriggerTierUp(previousTier, currentTier);
        }
        else
        {
            _events.TriggerTierDown(previousTier, currentTier);
        }
    }

    /// <summary>
    /// 근접 공격 피격 시 호출됩니다.
    /// </summary>
    private void HandleAttackAffect(Collider collider)
    {
        IncreaseHeatOnAttack(collider);
    }

    /// <summary>
    /// 차지 공격 피격 시 호출됩니다.
    /// </summary>
    private void HandleChargeAttackAffect(Collider collider)
    {
        IncreaseHeatOnChargeAttack(collider);
    }

    /// <summary>
    /// 패링 성공 시 호출됩니다.
    /// </summary>
    private void HandleParryAffect(Collider collider)
    {
        IncreaseHeatOnParrySuccess();
    }

    /// <summary>
    /// 원거리 공격 피격 시 호출됩니다.
    /// </summary>
    private void HandleRangedAttackAffect(Collider collider)
    {
        DecreaseHeatOnRangeAttack(collider);
    }

    /// <summary>
    /// 카운터 공격 피격 시 호출됩니다.
    /// </summary>
    private void HandleSecondCounterAttackAffect(Collider collider)
    {
        IncreaseHeatOnCounterAttack(collider);
    }

    /// <summary>
    /// 과열 종료 시 호출됩니다.
    /// </summary>
    private void HandleOverHeatFinish()
    {
        OverHeatFinish(); 
    }

    private void HandleBoostStart()
    {
        if(_stats.SkillData.IsMaxLevelBoost)
        {
            SetHeat(100);
        }
    }
    #endregion
}