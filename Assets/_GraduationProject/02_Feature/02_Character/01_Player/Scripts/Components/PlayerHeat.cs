using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어 열량 시스템 클래스
/// HeatSystem을 상속받아 플레이어의 열량 관리를 담당합니다.
/// 공격, 스킬 사용 등에 따라 열량이 축적되고, 티어별로 스탯 변화가 적용됩니다.
/// </summary>
public class PlayerHeat : HeatSystem
{
    /// <summary>
    /// 플레이어 컨텍스트 참조
    /// </summary>
    private PlayerContext _context;

    private float _lastMeleeAttackChargingTime = 0;

    /// <summary>
    /// 플레이어 열량 시스템 초기화
    /// </summary>
    /// <param name="context">플레이어 컨텍스트</param>
    public void Initialize(PlayerContext context)
    {
        _context = context;

        // TODO: 열량 시스템 이벤트 구독
        // TODO: 플레이어 ID로 열량 데이터 초기화

        _context.EventBus.OnAttack += AddHeatOnMeleeAttack;
        _context.EventBus.OnParrySuccess += AddHeatOnParry;
        _context.EventBus.OnMeleeAttackChargeStart += ChargeStart;
        _context.EventBus.OnMeleeAttackCharging += AddHeatOnMeleeAttackCharging;
    }

    /// <summary>
    /// 공격 시 열량 추가
    /// </summary>
    /// <param name="targets">충돌한 오브젝트들</param>
    private void AddHeatOnMeleeAttack(Collider[] targets)
    {
        foreach (Collider target in targets)
        {
            IHeatable heatable = target.GetComponent<IHeatable>();
            if (heatable != null)
            {
                SourceMap sourceMap = p_heatDataBase.GetSourceMap("OnMeleeHit", heatable.ActorType, -1);
                int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
                heatable.ChangeHeat(deltaHeat);
                Log.PrintColor(Color.red, $"target: {target.gameObject.name}, 열기 변화량: {deltaHeat}");
            }
        }
    }

    /// <summary>
    /// 패링 시 열량 추가
    /// </summary>
    private void AddHeatOnParry()
    {
        SourceMap sourceMap = p_heatDataBase.GetSourceMap("OnParrySuccess", ActorType, -1);
        int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
        ChangeHeat(deltaHeat);

        Log.PrintColor(Color.red, $"패링, 열기 변화량: {deltaHeat}");
    }

    private void ChargeStart()
    {
        _lastMeleeAttackChargingTime = Time.time;
    }

    /// <summary>
    /// 근거리 공격 차징 시 열량 추가
    /// </summary>
    private void AddHeatOnMeleeAttackCharging()
    {
        SourceMap sourceMap;
        sourceMap = p_heatDataBase.GetSourceMap("OnCharge", ActorType, -1);

        if (Time.time - _lastMeleeAttackChargingTime >= sourceMap.TickSecond)
        {
            int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
            ChangeHeat(deltaHeat);

            _lastMeleeAttackChargingTime = Time.time;
        }
    }

    public void OnDestroy()
    {
        if (_context?.EventBus != null)
        {
            _context.EventBus.OnAttack -= AddHeatOnMeleeAttack;
            _context.EventBus.OnParrySuccess -= AddHeatOnParry;
            _context.EventBus.OnMeleeAttackChargeStart -= ChargeStart;
            _context.EventBus.OnMeleeAttackCharging -= AddHeatOnMeleeAttackCharging;
        }
    }
}
