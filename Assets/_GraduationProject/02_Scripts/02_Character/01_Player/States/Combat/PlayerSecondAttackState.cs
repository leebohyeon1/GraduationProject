using BH_Lib.FSM;
using BH_Lib.Log;
using System;

/// <summary>
/// 플레이어의 두 번째 일반 공격 상태입니다.
/// </summary>
public class PlayerSecondAttackState : PlayerAttackBaseState
{
    public PlayerSecondAttackState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    protected override string p_animationTrigger => "SecondAttack";
    protected override Type p_nextAttackState => typeof(PlayerThirdAttackState);
    protected override PlayerAttackData p_AttackData => p_context.Stats.AttackDatas[1];
}