using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 차지 상태입니다.
/// </summary>
public class PlayerChargeState : BaseState<Player>
{
    private bool _isCharged = false; // 최소 차지 완료 여부

    public PlayerChargeState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Animator.SetBool("IsCharge", true);

        _isCharged = false;

        p_context.Events.TriggerChargeStart();
        p_context.Events.TriggerBattleStateChanged(true);
    }

    public override void OnUpdate()
    {
        p_context.Movement?.Move(Vector3.zero, 0f, 0f);

        // 입력에 따른 상태 전환
        if (!p_context.Input.AttackHeldInput)
        {
            if (_isCharged )
            {
                p_stateMachine.ChangeState<PlayerChargeAttackState>();   
            }
            else
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
 
            }
        }
        else if(p_context.Input.DodgeInput)
        {
            p_stateMachine.ChangeState <PlayerDodgeState>();
        }

        // 조준 방향으로 회전
        var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
        var moveInput = p_context.Input.MoveInput;
        var mousePosition = p_context.Input.MousePosition;
        p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);
    }

    public override void OnExit()
    {
        p_context.Animator.SetBool("IsCharge", false);
        p_context.Events.TriggerBattleStateChanged(true);
        p_context.Events.TriggerChargeCancel();
    }

}