using System;
using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

public class PlayerRangedAttackFireState : BaseState<PlayerContext>
{
    private Type _nextState; // 다음 상태를 저장할 변수
    protected bool p_canInput = false; // 입력 허용 플래그  

    public PlayerRangedAttackFireState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.EventBus.OnRangedAttackEnd += OnRangedAttackEndEvent;
        p_context.Animator.SetTrigger("RangedAttackFire");
        Log.Print("Player entered RangedAttackFireState");

        // 투사체 발사
        FireProjectile();
    }

    public override void OnUpdate()
    {
        if (p_context.Controller.DodgeInput && p_context.Movement.CanDodge())
        {
            _nextState = typeof(PlayerDodgeState);
        }
        else if (p_context.Controller.DefendInput)
        {
            _nextState = typeof(PlayerDefendState);
        }
        else if (p_context.Controller.AttackInput)
        {
            _nextState = typeof(PlayerFirstMeleeAttackState);
        }

    }

    public override void OnExit()
    {
        p_context.EventBus.OnRangedAttackEnd -= OnRangedAttackEndEvent;
        Log.Print("Player exited RangedAttackFireState");
    }

    private void FireProjectile()
    {
        p_context.EventBus.PublishRangedAttackStart();
    }

    private void OnRangedAttackEndEvent()
    {
         if (_nextState != null)
        {
            p_stateMachine.ChangeState(_nextState);
        }
        else
        { 
            // 아무 입력이 없었으면 Idle 상태로
            p_stateMachine.ChangeState<PlayerIdleState>();
        }
    }

}