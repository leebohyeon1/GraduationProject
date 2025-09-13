using System;
using System.Collections;
using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

public class PlayerChargeMeleeAttackState : PlayerMeleeAttackBaseState
{
    protected override string p_animationTrigger => "ChargeMeleeAttack";

    protected override Type p_nextAttackState => null;

    public PlayerChargeMeleeAttackState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
    : base(context, stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
         p_context.Event.ChargeMeleeAttack.OnFinished += AttackFinished;
    }

    public override void OnExit()
    {
        base.OnExit();

        p_context.MeleeAttack.ResetComboCount();
        p_context.Event.ChargeMeleeAttack.OnFinished -= AttackFinished;
    }

    protected override void HandleInput()
    {
        // 다음 상태가 아직 결정되지 않았고 입력이 허용된 경우
        if (p_canInput)
        {
            // 공격 중 입력 감지하여 다음 상태 저장
            if (p_context.Controller.DodgeInput && p_context.Movement.CanDodge())
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
