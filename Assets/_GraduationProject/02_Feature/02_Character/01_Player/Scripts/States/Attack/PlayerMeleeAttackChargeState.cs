using System.Threading;
using BH_Lib.FSM;
using UnityEngine;

public class PlayerMeleeAttackChargeState : BaseState<PlayerContext>
{
    private float _timer;

    public PlayerMeleeAttackChargeState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
        : base(context, stateMachine) { }


    public override void OnEnter()
    {
        base.OnEnter();

        p_context.Animator.SetBool("isMeleeAttackCharge", true);

        p_context.Event.Player.ChargeMeleeAttack.PublishStart();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        _timer += Time.deltaTime;
        if (!p_context.Controller.AttackHeldInput)
        {
            if (_timer >= p_context.Stats.MinChargeTime)
            {
                p_stateMachine.ChangeState<PlayerChargeMeleeAttackState>();
                return;
            }
            else
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
                return;
            }

        }
        
        // 에임 방향으로 회전
        var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
        var lookInput = p_context.Controller.LookInput;
        var mousePosition = p_context.Controller.MousePosition;
        p_context.Event.Player.PublishRotateToAttackDirection(deviceType, lookInput, mousePosition);
        p_context.Event.Player.ChargeMeleeAttack.PublishCharge();
    }

    public override void OnExit()
    {
        base.OnExit();
        
        p_context.Animator.SetBool("isMeleeAttackCharge", false);

    }

}
