using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;

/// <summary>
/// 플레이어의 첫 번째 카운터 공격 상태입니다.
/// </summary>
public class PlayerFirstCounterAttackState : PlayerAttackBaseState
{
    protected override string p_animationTrigger => "FirstCounterAttack";
    protected override Type p_nextAttackState => typeof(PlayerSecondCounterAttackState);
    protected override PlayerAttackData p_AttackData => p_context.Stats.CombatData.CounterAttackDatas[0];

    public PlayerFirstCounterAttackState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnAttackFinish += HandleAttackFinish;
        p_context.Events.OnAttackPerform += HandleAttackPerform;

        p_nextState = null;

        p_context.Animator.SetTrigger(p_animationTrigger);

        p_context.Combat.SetCanCounterAttack(false);
        p_context.Stats.IsCounterAttack = true;

        StartAttackMovement();
        p_context.Events.TriggerFirstCounterAttackStart();
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
    /// 공격 판정이 발생하는 시점에 호출됩니다.
    /// </summary>
    protected override void HandleAttackPerform()
    {
        p_context.Combat.ExcuteFirstCounterAttack(p_AttackData);
        p_context.Events.TriggerFirstCounterAttackAffect(p_context.Combat.CounterableTarget, p_context.Heat.CurrentTier);
    }

    /// <summary>
    /// 공격 애니메이션 종료 시 호출됩니다.
    /// </summary>
    protected override void HandleAttackFinish()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(p_AttackData.AttackDelay);

        sequence.AppendCallback(() =>
        {
            if (p_nextState != null)
            {
                // 다음 상태가 연계 카운터 공격이 아니면 카운터 상태 해제
                if(p_nextAttackState != p_nextState)
                {
                    p_context.Stats.IsCounterAttack = false;
                    p_context.Combat.ClearCounterTarget();
                }
                p_stateMachine.ChangeState(p_nextState);
            }
            else
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
            }
        });
    }
}