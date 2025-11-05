using BH_Lib.FSM;
using BH_Lib.Log;
using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 플레이어의 첫 번째 일반 공격 상태입니다.
/// </summary>
public class PlayerFirstAttackState : PlayerAttackBaseState
{
    public PlayerFirstAttackState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) { }

    protected override string p_animationTrigger => "FirstAttack";
    protected override Type p_nextAttackState => typeof(PlayerSecondAttackState);
    protected override PlayerAttackDataSO p_AttackData => p_context.Stats.AttackDatas[0];
}