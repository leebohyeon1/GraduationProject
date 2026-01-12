using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 플레이어의 첫 번째 일반 공격 상태입니다.
/// </summary>
public class PlayerAttackState : PlayerAttackBaseState
{
    public PlayerAttackState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) { }


    protected override string p_animationTrigger => "Attack";
    protected override PlayerAttackConfig p_AttackConfig => p_context.Stats.CurrentAttackData.AttackConfig;
}