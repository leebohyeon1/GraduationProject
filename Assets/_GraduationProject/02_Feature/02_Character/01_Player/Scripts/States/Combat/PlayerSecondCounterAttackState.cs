using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;

/// <summary>
/// 플레이어의 두 번째 카운터 공격 상태입니다.
/// </summary>
public class PlayerSecondCounterAttackState : PlayerAttackBaseState
{
    protected override string p_animationTrigger => "SecondCounterAttack";
    protected override Type p_nextAttackState => null;
    protected override PlayerAttackData p_AttackData => p_context.Stats.CombatData.CounterAttackDatas[1];

    public PlayerSecondCounterAttackState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnAttackFinish += HandleAttackFinish;
        p_context.Events.OnAttackPerform += HandleAttackPerform;

        p_nextState = null;

        p_context.Animator.SetTrigger(p_animationTrigger);

        StartAttackMovement();
        p_context.Events.TriggerSecondAttackStart();
    }

    public override void OnExit()
    {
        p_context.Events.OnAttackFinish -= HandleAttackFinish;
        p_context.Events.OnAttackPerform -= HandleAttackPerform;

        p_context.Animator.ResetTrigger(p_animationTrigger);

        DOTween.Kill(p_animationTrigger);

        p_nextState = null;
    }

    /// <summary>
    /// 공격 시 뒤로 물러나는 움직임을 시작합니다.
    /// </summary>
    protected override void StartAttackMovement()
    {
        float distance = p_AttackData.AttackMoveDistance;

        // 후방에 장애물이 있으면 이동 거리 조정
        if (Physics.Raycast(p_context.transform.position, -p_context.transform.forward, out var hitInfo, p_AttackData.AttackMoveDistance))
        {
            distance = hitInfo.distance - (p_context.GetComponent<Collider>().bounds.size.z / 2);
        }

        Vector3 targetPosition = p_context.transform.position - (p_context.transform.forward * distance);

        p_context.transform.DOMove(targetPosition, p_AttackData.AttackMoveDuration, false)
            .SetEase(p_AttackData.AttackMoveCurve).SetId(p_animationTrigger);
    }

    /// <summary>
    /// 공격 판정이 발생하는 시점에 호출됩니다.
    /// </summary>
    protected override void HandleAttackPerform()
    {
        p_context.Combat.ExcuteSecondCounterAttack(p_AttackData);
        p_context.Events.TriggerSecondCounterAttackAffect(p_context.Combat.CounterableTarget, p_context.Heat.CurrentTier);
    }

    /// <summary>
    /// 공격 애니메이션 종료 시 호출됩니다.
    /// </summary>
    protected override void HandleAttackFinish()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(p_AttackData.AttackDelay);
        p_context.Combat.ClearCounterTarget();
        p_context.Health.SetInvisible(false);

        sequence.AppendCallback(() =>
        {
            if (p_nextState != null)
            {
                p_stateMachine.ChangeState(p_nextState);
            }
            else
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
            }
        });
    }
}