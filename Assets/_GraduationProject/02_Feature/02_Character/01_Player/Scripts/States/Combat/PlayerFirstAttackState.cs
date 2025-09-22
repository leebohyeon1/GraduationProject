using BH_Lib.FSM;
using System;


public class PlayerFirstAttackState : PlayerAttackBaseState
{
    public PlayerFirstAttackState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) { }

    protected override string p_animationTrigger => "FirstAttack";

    protected override Type p_nextAttackState => typeof(PlayerSecondAttackState);

    protected override PlayerAttackData p_AttackData => p_context.DataBase.RuntimeData.CombatData.AttackDatas[0];

    public override void OnEnter()
    {
        base.OnEnter();

        p_context.Events.TriggerFirstAttackStart();
    }

}

