using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System.Threading;
using UnityEngine;

/// <summary>
/// 플레이어의 방어 상태입니다.
/// </summary>
public class PlayerDefendState : BaseState<Player>
{
    public PlayerDefendState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
        var moveInput = p_context.Input.MoveInput;
        var mousePosition = p_context.Input.MousePosition;
        p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);

        p_context.Animator.SetBool("IsDefending", true);
        p_context.Combat.SetDefending(true);
        p_context.Events.TriggerBattleStateChanged(true);
    }

    public override void OnUpdate()
    {
        Vector3 moveDirection = new Vector3(p_context.Input.MoveInput.x, 0, p_context.Input.MoveInput.y).normalized;
        p_context.Movement.Move(moveDirection, p_context.Stats.Data.MoveSpeed, p_context.Stats.Data.RotateSpeed);

        // 입력에 따른 상태 전환
        if (!p_context.Input.DefendInput)
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
            p_context.Combat.SetDefending(false);
        }
        else if (p_context.Input.DodgeInput)
        {
            p_stateMachine.ChangeState<PlayerDodgeState>();
            p_context.Combat.SetDefending(false);
        }
    }

    public override void OnExit()
    {
        DOTween.Kill(this);
        p_context.Animator.SetBool("IsDefending", false);
        p_context.Events.TriggerBattleStateChanged(true);
    }

}