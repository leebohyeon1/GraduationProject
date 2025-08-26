using System;
using BH_Lib.FSM;
using UnityEngine;

public class PlayerSecondAttackState : PlayerAttackBaseState
{
    protected override string p_animationTrigger => "SecondAttack";

    protected override Type p_nextAttackState => null;

    public PlayerSecondAttackState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
        : base(context, stateMachine) { }
}