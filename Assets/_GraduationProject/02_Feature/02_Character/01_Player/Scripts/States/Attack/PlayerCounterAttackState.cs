using System;
using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어 카운터 공격 상태
/// </summary>
public class PlayerCounterAttackState : PlayerMeleeAttackBaseState
{
    /// <summary>카운터 공격 애니메이션 트리거</summary>
    protected override string p_animationTrigger => "CounterAttack";

    /// <summary>다음 공격 상태 </summary>
    protected override Type p_nextAttackState => null;

    /// <summary>
    /// 카운터 공격 상태 생성자
    /// </summary>
    public PlayerCounterAttackState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();

        Log.Print("Player entered CounterAttack state");
        p_context.Event.CounterAttack.OnFinished += AttackFinished;
        p_context.Event.CounterAttack.PublishStart();
    }

    public override void OnExit()
    {
        base.OnExit();

        Log.Print("Player exited CounterAttack state");
        p_context.Event.CounterAttack.OnFinished -= AttackFinished;
    }
}