using BH_Lib.FSM;
using System;


public class PlayerSecondAttackState : PlayerAttackBaseState
{
    public PlayerSecondAttackState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    protected override string p_animationTrigger => "SecondAttack";

    protected override Type p_nextAttackState => typeof(PlayerThirdAttackState);

    protected override PlayerAttackData p_AttackData => p_context.RuntimeData.CombatData.AttackDatas[1];

    public override void OnEnter()
    {
        base.OnEnter();

        p_context.Events.TriggerSecondAttackStart();
    }

}


