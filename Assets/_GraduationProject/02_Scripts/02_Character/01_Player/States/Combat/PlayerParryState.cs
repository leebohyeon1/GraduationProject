using BH_Lib.FSM;
using BH_Lib.Log;
using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 플레이어의 일반 상쇄 상태입니다.
/// </summary>
public class PlayerParryState : PlayerAttackBaseState
{
    public PlayerParryState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) { }


    protected override string p_animationTrigger => "Parry";
    protected override PlayerAttackConfig p_AttackConfig => p_context.Stats.CurrentAttackData.AttackConfig;


    /// <summary>
    /// 공격 중 입력을 처리하여 다음 상태를 결정합니다.
    /// </summary>
    protected override void HandleInput()
    {
        if (p_nextState != null || !_canInput)
        {
            return;
        }

        if (p_context.Stats.CanNextAttack && p_context.Stamina.CheckStamina())
        {
            if (p_context.Input.AttackInput)
            {
                p_context.Events.TriggerChangedNextAttackState();
                p_stateMachine.ChangeState(typeof(PlayerAttackState));
            }
            else if (p_context.Input.AttackHeldInput)
            {
                p_stateMachine.ChangeState(typeof(PlayerChargeState));
            }
            else if (p_context.Input.ParryInput)
            {
                p_stateMachine.ChangeState(typeof(PlayerParryState));
            }

        }
        else if (p_context.Input.DodgeInput && p_context.Stamina.CheckStamina())
        {

            p_nextState = typeof(PlayerDodgeState);
        }

    }
}