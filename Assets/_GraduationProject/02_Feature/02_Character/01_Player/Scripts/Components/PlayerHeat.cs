using BH_Lib.Log;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

public class PlayerHeat : HeatSystem
{
    private OverHeatDataSO _overHeatData;
    private float _heatTierTimer;
    private bool _isOverHeat;   

    public bool IsOverHeat => _isOverHeat;

    private Sequence _overheatSequence;

    /// <summary>
    /// 열기 시스템 초기화
    /// </summary>
    /// <param name="sourceMapDatabaseSO">소스맵 데이터베이스</param>
    /// <param name="tierStatDatabaseSO">티어 스탯 데이터베이스</param>
    public void Initialize(SourceMapDatabaseSO sourceMapDatabaseSO, TierStatDatabaseSO tierStatDatabaseSO, OverHeatDataSO overHeatDataSO)
    {
        p_sourceMapDataBase = sourceMapDatabaseSO;
        p_tierStatDatabase = tierStatDatabaseSO;
        _overHeatData = overHeatDataSO;
    }

    #region SetHeat
    /// <summary>
    /// 열량 변경 함수
    /// </summary>
    /// <param name="amount"> 열기 변화량 </param>
    public override void ChangeHeat(int amount)
    {
        if (amount == 0 && IsHeatLock)
        {
            return;
        }

        int previousTier = GetTier();
        int previousHeat = p_currentHeat;

        p_currentHeat = Mathf.Clamp(p_currentHeat + amount, 0, MaxHeat);


        if (previousHeat != p_currentHeat)
        {
            TriggerOnHeatChanged(previousHeat);
        }

        int newTier = GetTier();
        if (previousTier != newTier)
        {
            TriggerOnTierChanged(previousTier);
        }

        if(CurrentHeat >= MaxHeat && !IsOverHeat)
        {
            OverHeat();
        }
    }

    /// <summary>
    /// 열량 설정 함수
    /// </summary>
    /// <param name="amount"> 열기 설정값 </param>
    public override void SetHeat(int amount)
    {
        if (IsHeatLock)
        {
            return;
        }

        int previousTier = GetTier();
        int previousHeat = p_currentHeat;

        p_currentHeat = Mathf.Clamp(amount, 0, MaxHeat);

        if (previousHeat != p_currentHeat)
        {
            TriggerOnHeatChanged(previousHeat);
        }

        int newTier = GetTier();
        if (previousTier != newTier)
        {
            TriggerOnTierChanged(previousTier);
        }

        if (CurrentHeat >= MaxHeat && !IsOverHeat)
        {
            OverHeat();
        }
    }
    #endregion

    #region OverHeat
    /// <summary>
    /// 오버히트 시작
    /// </summary>
    protected override void OverHeat()
    {
        if (_overheatSequence != null && _overheatSequence.IsActive())
        {
            return;
        }

        _overheatSequence = DOTween.Sequence();
        _overheatSequence.SetDelay(_overHeatData.DelaySecond)
            .AppendCallback(() =>
            {
                _heatTierTimer = Time.time;
                _isOverHeat = true;
                SetHeatLock(true);
            });
    }

    /// <summary>
    /// 오버히트 효과 작동할 수 있는지
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

    public void OverHeatFinish()
    {
        _overheatSequence?.Kill();

        if (_isOverHeat)
        {
            _isOverHeat = false;
            SetHeatLock(false);
        }
    }
    #endregion

    /// <summary>
    /// 근접 공격 시 대상 열기 증가 처리
    /// </summary>
    /// <param name="collider">타격 대상 콜라이더</param>
    public void IncreaseHeatOnAttack(Collider collider)
    {
        IHeatable heatable = collider.GetComponent<IHeatable>();

        if (heatable != null)
        {
            SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnMeleeHit", heatable.ActorType, -1);
            int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;

            heatable.ChangeHeat(deltaHeat);

            Log.PrintColor(Color.red, $"대상: {collider.gameObject.name}, 열기 변화량: {deltaHeat}");
        }
    }

    /// <summary>
    /// 차지 게이지에 따른 열기 증가 처리
    /// </summary>
    /// <param name="sourceMap">소스맵 데이터</param>
    /// <param name="chargeGuage">차지 게이지 값</param>
    public void IncreaseHeatOnCharge(SourceMap sourceMap, float chargeGuage)
    {
        if (chargeGuage >= CurrentHeat)
        {
            SetHeat(Mathf.FloorToInt(chargeGuage));
        }
    }

    /// <summary>
    /// 차지 공격 시 대상 열기 증가 처리
    /// </summary>
    /// <param name="collider">타격 대상 콜라이더</param>
    public void IncreaseHeatOnChargeAttack(Collider collider)
    {
        IHeatable heatable = collider.GetComponent<IHeatable>();
        if (heatable != null)
        {
            SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnChargeAttack", heatable.ActorType, CurrentTier);
            int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
            heatable.ChangeHeat(deltaHeat);

            Log.PrintColor(Color.red, $"대상: {collider.gameObject.name}, 열기 변화량: {deltaHeat}");
        }
        SetHeat(0);
    }

    /// <summary>
    /// 원거리 공격 시 대상 열기 감소 처리
    /// </summary>
    /// <param name="collider">타격 대상 콜라이더</param>
    public void DecreaseHeatOnRangeAttack(Collider collider)
    {
        IHeatable heatable = collider.GetComponent<IHeatable>();
        if (heatable != null)
        {
            SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnIceBallSuccess", heatable.ActorType, -1);
            int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;

            Log.PrintColor(Color.red, $"대상: {heatable.ActorType}, 열기 변화량: {deltaHeat}");
            heatable.ChangeHeat(deltaHeat);
        }
    }

    /// <summary>
    /// 패리 성공 시 열기 증가 처리
    /// </summary>
    public void IncreaseHeatOnParrySuccess()
    {
        SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnParrySuccess", -1);
        int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
            
        ChangeHeat(deltaHeat);
    }

    /// <summary>
    /// 전투에서 나옴으로 인한 열기 감소 함수
    /// </summary>
    public void DecreaseHeatOnBattleOut()
    {
        if(CurrentHeat <= 0)
        {
            return;
        }

        SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnBattleOut", -1);
        int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
            
        ChangeHeat(deltaHeat);
    }

    /// <summary>
    /// 카운터 공격으로 인한 열기 상승 함수
    /// </summary>
    public void IncreaseHeatOnCounterAttack(Collider collider)
    {
        if(collider.TryGetComponent<IHeatable>(out var heatable))
        {
            SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnCounterSuccess", heatable.ActorType, CurrentTier);
            int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;

            Log.PrintColor(Color.red, $"대상: {heatable.ActorType}, 열기 변화량: {deltaHeat}");
            heatable.ChangeHeat(deltaHeat);
        }

        SetHeat(0);
    }
}

/// <summary>
/// 플레이어 열기 시스템을 관리하는 클래스
/// </summary>
public class PlayerHeatManager : IDisposable
{
    private PlayerHeat _heat;
    private PlayerEvents _events;
    private bool _disposed = false; // 중복 Dispose 방지

    public PlayerHeatManager(PlayerHeat heat, PlayerEvents events)
    {
        _heat = heat;
        _events = events;

        // 이벤트 구독
        _events.OnBattleStateChaged += HandleBattleSateChanged;
        _heat.OnTierChanged += HandleHeatChanged;
        _events.OnAttackAffect += HandleAttackAffect;
        _events.OnChargeAttackAffect += HandleChargeAttackAffect;
        _events.OnParryAffect += HandleParryAffect;
        _events.OnRangedAttackAffect += HandleRangedAttackAffect;
        _events.OnSecondCounterAttackAffect += HandleSecondCounterAttackAffect; 
        _events.OnOverHeatFinish += HandleOverHeatFinish;
    }

    /// <summary>
    /// 리소스 정리 및 이벤트 구독 해제
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // 이벤트 구독 해제
        _events.OnBattleStateChaged -= HandleBattleSateChanged;
        _heat.OnTierChanged -= HandleHeatChanged;
        _events.OnAttackAffect -= HandleAttackAffect;
        _events.OnChargeAttackAffect -= HandleChargeAttackAffect;
        _events.OnParryAffect -= HandleParryAffect;
        _events.OnRangedAttackAffect -= HandleRangedAttackAffect;
        _events.OnSecondCounterAttackAffect -= HandleSecondCounterAttackAffect;
        _events.OnOverHeatFinish -= HandleOverHeatFinish;

        _disposed = true;
    }

    /// <summary>
    /// 전투 상황을 벗어났을 때 상황
    /// </summary>
    private Sequence _battleOutSequence;
    /// <summary>
    /// 전투 상태 변경 이벤트 처리
    /// </summary>
    /// <param name="isBattleState">전투 상태 여부</param>
    private void HandleBattleSateChanged(bool isBattleState)
    {
        _battleOutSequence?.Kill();

        if(!isBattleState)
        {
            _heat.SetHeatLock(false);

            _battleOutSequence = DOTween.Sequence()
                .AppendCallback(_heat.DecreaseHeatOnBattleOut)
                .SetDelay(1f)
                .SetLoops(-1, LoopType.Restart);

            _battleOutSequence.Play();
        }
    }

    /// <summary>
    /// 열기 변화 이벤트 처리
    /// </summary>
    /// <param name="previousTier">이전 티어</param>
    /// <param name="currentTier">현재 티어</param>
    private void HandleHeatChanged(int previousTier, int currentTier)
    {
        // 열기 티어 변경 여부 확인
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
    /// 근접 공격 시 열기 효과 처리
    /// </summary>
    /// <param name="collider">타격 대상 콜라이더</param>
    private void HandleAttackAffect(Collider collider)
    {
        _heat.IncreaseHeatOnAttack(collider);
    }

    /// <summary>
    /// 차지 공격 시 열기 효과 처리
    /// </summary>
    /// <param name="collider">타격 대상 콜라이더</param>
    private void HandleChargeAttackAffect(Collider collider)
    {
        _heat.IncreaseHeatOnChargeAttack(collider);
    }

    /// <summary>
    /// 패리 성공 시 열기 효과 처리
    /// </summary>
    /// <param name="collider">패리 대상 콜라이더</param>
    private void HandleParryAffect(Collider collider)
    {
        _heat.IncreaseHeatOnParrySuccess();
    }

    /// <summary>
    /// 원거리 공격 시 열기 효과 처리
    /// </summary>
    /// <param name="collider">타격 대상 콜라이더</param>
    private void HandleRangedAttackAffect(Collider collider)
    {
        _heat.DecreaseHeatOnRangeAttack(collider);
    }

    /// <summary>
    /// 카운터 공격 시 열기 효과 처리
    /// </summary>
    /// <param name="collider">타격 대상 콜라이더</param>
    private void HandleSecondCounterAttackAffect(Collider collider)
    {
        _heat.IncreaseHeatOnCounterAttack(collider);
    }

    /// <summary>
    /// 오버 히트 종료 시 
    /// </summary>
    private void HandleOverHeatFinish()
    {
        _heat.OverHeatFinish(); 
    }
}
