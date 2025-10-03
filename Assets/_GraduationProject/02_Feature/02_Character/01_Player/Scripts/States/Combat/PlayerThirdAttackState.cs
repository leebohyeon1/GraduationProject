using BH_Lib.FSM;
using System;

public class PlayerThirdAttackState : PlayerAttackBaseState
{
    public PlayerThirdAttackState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    protected override string p_animationTrigger => "ThirdAttack";

    protected override Type p_nextAttackState => null;

    protected override PlayerAttackData p_AttackData => p_context.Stats.CombatData.AttackDatas[2];

    public override void OnEnter()
    {
        base.OnEnter();

        p_context.Events.TriggerThirdAttackStart();
    }
}


