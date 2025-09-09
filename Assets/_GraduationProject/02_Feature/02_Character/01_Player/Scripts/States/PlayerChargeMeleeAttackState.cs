using System;
using System.Collections;
using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

public class PlayerChargeMeleeAttackState : PlayerMeleeAttackBaseState
{
    protected override string p_animationTrigger => "ChargeMeleeAttack";

    protected override Type p_nextAttackState => typeof(PlayerFirstMeleeAttackState);

    public PlayerChargeMeleeAttackState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
    : base(context, stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();

    }

    public override void OnExit()
    {
        base.OnExit();

        p_context.MeleeAttack.ResetComboCount();
    }

    protected override void HandleInput()
    {
        // 다음 상태가 아직 결정되지 않았고 입력이 허용된 경우
        if (p_canInput)
        {
            // 공격 중 입력 감지하여 다음 상태 저장
            if (p_nextAttackState != null && p_context.Controller.AttackInput)
            {
                p_nextState = p_nextAttackState;
            }
            else if (p_context.Controller.DodgeInput && p_context.Movement.CanDodge())
            {
                p_nextState = typeof(PlayerDodgeState);
            }
            else if (p_context.Controller.DefendInput)
            {
                p_nextState = typeof(PlayerDefendState);
            }
            else if (p_context.Controller.RangedAttackInput)
            {
                p_nextState = typeof(PlayerRangedAttackChargeState);
            }


            if (p_nextState != null)
            {
                Log.PrintColor(Color.skyBlue, $"[PlayerAttackBaseState] 다음 상태: {p_nextState}");
            }

        }
    }
    

}
