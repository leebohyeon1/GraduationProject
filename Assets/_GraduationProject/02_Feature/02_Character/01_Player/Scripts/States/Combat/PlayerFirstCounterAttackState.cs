using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;

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

        p_nextState = null; // 다음 상태 초기화

        p_context.Animator.SetTrigger(p_animationTrigger);  // 공격 애니메이션 실행

        p_context.Combat.SetCanCounterAttack(false);
        p_context.Health.SetInvisible(true);

        // 공격 시 전진 이동 실행
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

    protected override void HandleAttackPerform()
    {
        p_context.Combat.ExcuteFirstCounterAttack(p_AttackData);
        p_context.Events.TriggerFirstCounterAttackAffect(
            p_context.Combat.CounterableTarget, 
            p_context.Heat.CurrentTier);
    }

    /// <summary>
    /// 공격 애니메이션 이벤트 핸들러
    /// 공격이 완료되면 다른 상태로 전환
    /// </summary>
    protected override void HandleAttackFinish()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(p_AttackData.AttackDelay);

        sequence.AppendCallback(() =>
        {
            if (p_nextState != null)
            {
                if(p_nextAttackState != p_nextState)
                {
                    p_context.Health.SetInvisible(false);
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
