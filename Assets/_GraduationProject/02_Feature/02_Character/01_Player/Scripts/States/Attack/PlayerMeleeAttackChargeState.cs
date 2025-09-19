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

<<<<<<< Updated upstream
        p_context.EventBus.PublishAllowAttackInput();
        p_context.EventBus.PublishMeleeAttackChargeStart();
=======
        p_context.Event.MeleeAttackCharge.PublishStart();
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
        p_context.EventBus.PublishRotateToAttackDirection(deviceType, lookInput, mousePosition);
        p_context.EventBus.PublishMeleeAttackCharging();
=======
        p_context.Event.PublishRotateToAttackDirection(deviceType, lookInput, mousePosition);
        p_context.Event.MeleeAttackCharge.PublishPerform();
>>>>>>> Stashed changes
    }

    public override void OnExit()
    {
        base.OnExit();
        
        p_context.Animator.SetBool("isMeleeAttackCharge", false);

    }

}
