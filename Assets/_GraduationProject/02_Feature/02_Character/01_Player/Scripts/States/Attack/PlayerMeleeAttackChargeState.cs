using System.Threading;
using BH_Lib.FSM;
using UnityEngine;

public class PlayerMeleeAttackChargeState : BaseState<PlayerContext>
{
    private float _timer;
    private bool _isCharged = false;

    public PlayerMeleeAttackChargeState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
        : base(context, stateMachine) { }


    public override void OnEnter()
    {
        base.OnEnter();

        p_context.Animator.SetBool("isMeleeAttackCharge", true);

        p_context.Event.MeleeAttackCharge.PublishStart(p_context.Combat.ChargeStartEffectPoint.position);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        _timer += Time.deltaTime;

        if (!_isCharged && _timer >= p_context.Stats.MinChargeTime)
        {
            _isCharged = true;
            p_context.Event.MeleeAttackCharge.PublishFinished(p_context.Combat.ChargeFinishEffectPoint.position);
        }

        if (!p_context.Controller.AttackHeldInput)
        {
            if (_isCharged)
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
        p_context.Event.PublishRotateToAttackDirection(deviceType, lookInput, mousePosition);

        p_context.Event.MeleeAttackCharge.PublishPerform(p_context.Owner.transform.position);
    }

    public override void OnExit()
    {
        base.OnExit();
        
        p_context.Animator.SetBool("isMeleeAttackCharge", false);

    }

}
