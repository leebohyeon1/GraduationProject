using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어 열량 시스템 클래스
/// HeatSystem을 상속받아 플레이어의 열량 관리를 담당합니다.
/// 공격, 스킬 사용 등에 따라 열량이 축적되고, 티어별로 스탯 변화가 적용됩니다.
/// </summary>
public class PlayerHeat : HeatSystem, IPlayerHeatable
{
    /// <summary>
    /// 플레이어 컨텍스트 참조
    /// </summary>
    private PlayerContext _context;

    private float _lastMeleeAttackChargingTime = 0;
    private float _lastRestTime = 0;
    private int _chargeGuage = 0;

    private void Update()
    {
        MinusHeatOnRest();
    }

    /// <summary>
    /// 플레이어 열량 시스템 초기화
    /// </summary>
    /// <param name="context">플레이어 컨텍스트</param>
    public void Initialize(PlayerContext context)
    {
        _context = context;

        // TODO: 열량 시스템 이벤트 구독
        // TODO: 플레이어 ID로 열량 데이터 초기화
        _context.Event.MeleeAttack.OnAffect += HandleMeleeAttackAffect;
        _context.Event.Parry.OnAffect += HandleParryAffect;

        _context.Event.MeleeAttackCharge.OnStart += HandleMeleeAttackChargeStart;
        _context.Event.MeleeAttackCharge.OnPerform += HandleMeleeAttackChargePerform;

        _context.Event.RangedAttack.OnAffect += HandleRangedAttackAffect;

        _context.Event.Skill.OnPerform += HandleSkillPerform;
    }

    public void OnDisable()
    {
        _context.Event.MeleeAttack.OnAffect -= HandleMeleeAttackAffect;
        _context.Event.Parry.OnAffect -= HandleParryAffect;

        _context.Event.MeleeAttackCharge.OnStart -= HandleMeleeAttackChargeStart;
        _context.Event.MeleeAttackCharge.OnPerform -= HandleMeleeAttackChargePerform;

        _context.Event.RangedAttack.OnAffect -= HandleRangedAttackAffect;

        _context.Event.Skill.OnPerform -= HandleSkillPerform;
    }

    #region Feedback Handlers
    private void HandleMeleeAttackAffect(Vector3 position, Collider target)
    {
        AddHeatOnMeleeAttack(target);
    }

    private void HandleParryAffect(Vector3 position, Collider collider)
    {
        AddHeatOnParry();
    }

    private void HandleMeleeAttackChargeStart(Vector3 position)
    {
        ChargeStart();
    }

    private void HandleMeleeAttackChargePerform(Vector3 position)
    {
        AddHeatOnMeleeAttackCharging();
    }

    private void HandleRangedAttackAffect(Vector3 position, Collider target)
    {
        Log.PrintColor(Color.red, "원거리 공격 열량 감소 처리");
        MinusHeatOnRangedAttack(target);
    }

    private void HandleSkillPerform(Vector3 position)
    {
        MinusHeatOnSkill();
    }
    #endregion

    /// <summary>
    /// 공격 시 열량 추가
    /// </summary>
    /// <param name="targets">충돌한 오브젝트들</param>
    private void AddHeatOnMeleeAttack(Collider target)
    {
        IHeatable heatable = target.GetComponent<IHeatable>();
        if (heatable != null)
        {
            SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnMeleeHit", heatable.ActorType, -1);
            int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
            heatable.ChangeHeat(deltaHeat);
            Log.PrintColor(Color.red, $"target: {target.gameObject.name}, 열기 변화량: {deltaHeat}");
        }
    }

    /// <summary>
    /// 패링 시 열량 추가
    /// </summary>
    private void AddHeatOnParry()
    {
        SourceMap sourceMap = p_sourceMapDataBase.GetSourceMap("OnParrySuccess", ActorType, -1);
        int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;
        ChangeHeat(deltaHeat);

        Log.PrintColor(Color.red, $"패링, 열기 변화량: {deltaHeat}");
    }

    private void ChargeStart()
    {
        _lastMeleeAttackChargingTime = Time.time;
        _chargeGuage = 0;
    }

    /// <summary>
    /// 근거리 공격 차징 시 열량 추가
    /// </summary>
    private void AddHeatOnMeleeAttackCharging()
    {
        SourceMap sourceMap;
        sourceMap = p_sourceMapDataBase.GetSourceMap("OnCharge", ActorType, -1);

        if (Time.time - _lastMeleeAttackChargingTime >= (float)(sourceMap.TickSecond / sourceMap.DeltaHeat))
        {
            _chargeGuage += (int)sourceMap.HeatChangeType;

            if (_chargeGuage >= CurrentHeat)
            {
                SetHeat(_chargeGuage); ;
            }

            _lastMeleeAttackChargingTime = Time.time;
        }
    }

    /// <summary>
    /// 휴식으로 인한 열기 감소
    /// </summary>
    private void MinusHeatOnRest()
    {
        if (_context.Combat.IsRest)
        {
            SourceMap sourceMap;
            sourceMap = p_sourceMapDataBase.GetSourceMap("OnBattleOut", ActorType, -1);

            if (sourceMap.DeltaHeat > 0 && Time.time - _lastRestTime >= (float)(sourceMap.TickSecond / sourceMap.DeltaHeat))
            {
                ChangeHeat((int)sourceMap.HeatChangeType);
                _lastRestTime = Time.time;
            }
        }
    }

    /// <summary>
    /// 원거리 공격 시 열량 감소
    /// </summary>
    /// <param name="target"> 충돌한 오브젝트 </param>
    private void MinusHeatOnRangedAttack(Collider target)
    {
        IHeatable heatable = target.GetComponent<IHeatable>();
        if (heatable != null)
        {
            SourceMap sourceMap;
            sourceMap = p_sourceMapDataBase.GetSourceMap("OnIceBallSuccess", heatable.ActorType, -1);

            int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;

            Log.PrintColor(Color.red, $"target: {heatable.ActorType}, 열기 변화량: {deltaHeat}");
            heatable.ChangeHeat(deltaHeat);
        }

    }

    /// <summary>
    /// 스킬 사용 시 열량 감소
    /// </summary>
    private void MinusHeatOnSkill()
    {
        SourceMap sourceMap;
        sourceMap = p_sourceMapDataBase.GetSourceMap("OnIceBallSuccess", -1);
        int deltaHeat = (int)sourceMap.HeatChangeType * sourceMap.DeltaHeat;

        ChangeHeat(deltaHeat);

    }

    public int GetCostMana(string id, int tier = -1)
    {
        SourceMap data = p_sourceMapDataBase.GetSourceMap(id, tier);
        return data.ManaCost;
    }   
}