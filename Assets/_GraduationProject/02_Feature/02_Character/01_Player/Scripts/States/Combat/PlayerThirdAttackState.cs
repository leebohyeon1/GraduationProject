using BH_Lib.FSM;
using System;

/// <summary>
/// 플레이어의 세 번째 일반 공격 상태입니다.
/// </summary>
public class PlayerThirdAttackState : PlayerAttackBaseState
{
    public PlayerThirdAttackState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    protected override string p_animationTrigger => "ThirdAttack";
    protected override Type p_nextAttackState => null; // 마지막 공격이므로 다음 연계 공격 없음
    protected override PlayerAttackData p_AttackData => p_context.Stats.AttackDatas[2];

    public override void OnEnter()
    {
        base.OnEnter();
        p_context.Events.TriggerThirdAttackStart();
    }
}