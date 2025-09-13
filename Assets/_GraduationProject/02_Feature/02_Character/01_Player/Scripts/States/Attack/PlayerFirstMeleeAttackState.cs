using System;
using BH_Lib.FSM;
using UnityEngine;

/// <summary>
/// 플레이어 공격 상태
/// 공격 중에는 이동이 제한되며, 공격이 완료되면 다른 상태로 전환
/// </summary>
public class PlayerFirstMeleeAttackState : PlayerMeleeAttackBaseState
{
    protected override string p_animationTrigger => "FirstAttack";

    protected override Type p_nextAttackState => typeof(PlayerSecondMeleeAttackState);

    public PlayerFirstMeleeAttackState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();

        p_context.Event.MeleeAttack.OnFinished += AttackFinished;

        p_context.Event.MeleeAttack.PublishStart();
    }

    public override void OnExit()
    {
        base.OnExit();

        p_context.Event.MeleeAttack.OnFinished -= AttackFinished;
    }
}