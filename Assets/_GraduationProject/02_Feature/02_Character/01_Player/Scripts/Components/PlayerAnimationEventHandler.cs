using BH_Lib.Log;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 플레이어 애니메이션 이벤트 핸들러 클래스
/// Animator의 애니메이션 이벤트를 받아서 PlayerEventBus로 전달하는 역할을 담당합니다.
/// Unity의 Animation Event 시스템과 게임 로직을 연결하는 브릿지 역할을 합니다.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimationEventHandler : MonoBehaviour
{
    private PlayerContext _context;
    /// <summary>플레이어 이벤트 버스 참조</summary>
    private PlayerEventChannel _event;

    /// <summary>
    /// 애니메이션 이벤트 핸들러 초기화
    /// </summary>
    /// <param name="event">플레이어 이벤트 매니저</param>
    public void Initialize(PlayerContext context)
    {
        _context = context;
        _event = _context.Event;
    }


    /// <summary>
    /// 애니메이션 이벤트: 발소리 시점
    /// </summary>
    public void TriggerFootstep()
    {
        _event.PublishFootstep();
    }

    /// <summary>
    /// 애니메이션 이벤트: 멈춤 시점
    /// </summary>
    public void TriggerStop()
    {
        _event.PublishMoveStop();
    }

    /// <summary>
    /// 애니메이션 이벤트: 회피 애니메이션 종료 시점
    /// </summary>
    public void TriggerDodgeEnd()
    {
        _event.Dodge.PublishFinished(transform.position);
    }

    /// <summary>
    /// 애니메이션 이벤트: 패링 시점
    /// </summary>
    public void TriggerParry()
    {
        _event.Parry.PublishPerform(_context.Combat.ChargeStartEffectPoint.position);
    }

    #region MeleeAttack
    /// <summary>
    /// 애니메이션 이벤트: 공격 시점
    /// </summary>
    public void TriggerMeleeAttack()
    {
        _event.MeleeAttack.PublishPerform(transform.position);
    }

    /// <summary>
    /// 애니메이션 이벤트: 공격이 끝난 시점
    /// </summary>
    public void TriggerMeleeAttackFinished()
    {
        Log.PrintColor(Color.aliceBlue, "공격 종료!");
        _event.MeleeAttack.PublishFinished(transform.position);
    }
    #endregion

    #region RangedAttack
    /// <summary>
    /// 애니메이션 이벤트: 원거리 공격 종료 시점
    /// </summary>
    public void TriggerRangedAttackFinished()
    {
        _event.RangedAttack.PublishFinished(_context.RangedAttack.ProjectileSpawnPosition);
    }
    #endregion

    #region ChargeMeleeAttack
    /// <summary>
    /// 애니메이션 이벤트: 근거리 차징 공격 시점
    /// </summary>
    public void TriggerChargeMeleeAttack()
    {
        _event.ChargeMeleeAttack.PublishPerform(transform.position);
    }

    /// <summary>
    /// 애니메이션 이벤트: 근거리 차징 공격 종료 시점
    /// </summary>    
    public void TriggerChargeMeleeAttackFinished()
    {
        _event.ChargeMeleeAttack.PublishFinished(_context.Combat.ChargeAttackFinishEffectPoint.position);
    }
    #endregion

    #region CounterAttack
    /// <summary>
    /// 애니메이션 이벤트: 카운터 공격 시점
    /// </summary>
    public void TriggerCounterAttack()
    {
        _event.CounterAttack.PublishPerform(transform.position);
    }

    /// <summary>
    /// 애니메이션 이벤트: 카운터 공격 종료 시점
    /// </summary>
    public void TriggerCounterAttackFinished()
    {
        _event.CounterAttack.PublishFinished(_context.Combat.CounterAttackFinishEffectPoint.position);
    }

    #endregion

    #region Skill
    public void TriggerSkill()
    {
        _event.Skill.PublishPerform(_context.Combat.SkillEffectPoint.position);
    }

    public void TriggerSkillFinished()
    {
        _event.Skill.PublishFinished(_context.Combat.SkillEffectPoint.position);
    }

    #endregion
}
