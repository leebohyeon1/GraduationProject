using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어 이동 상태
/// 이동 입력에 따라 활성화되는 상태
/// </summary>
public class PlayerMoveState : BaseState<Player>
{
    public PlayerMoveState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Animator.SetBool("IsMoving", true);
    }

    public override void OnFixedUpdate()
    {
        // 이동 처리 (상태 전환은 StateMachine에서 별도 처리)
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (p_context.Movement != null && p_context.Input.MoveInput != Vector2.zero)
        {
            // 2D 입력을 3D 방향 좌표로 변환
            Vector3 moveDirection = new Vector3(p_context.Input.MoveInput.x, 0, p_context.Input.MoveInput.y);
            p_context.Movement.Move(moveDirection, p_context.Stats.MoveSpeed, p_context.Stats.RotateSpeed);
        }
    }

    public override void OnExit()
    {
        p_context.Animator.SetBool("IsMoving", false);
    }
}