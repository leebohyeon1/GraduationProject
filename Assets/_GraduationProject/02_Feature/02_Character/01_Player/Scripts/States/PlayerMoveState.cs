using BH_Lib.FSM;
using UnityEngine;

/// <summary>
/// 플레이어 이동 상태
/// 이동 입력이 있을 때 활성화되는 상태
/// </summary>
public class PlayerMoveState : BaseState<Player>
{
    public PlayerMoveState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.PlayerAnimator.SetBool("IsMoving", true);

        Debug.Log("Player entered Move state");
    }

    public override void OnUpdate()
    {
        // 이동 처리 (상태 전환은 StateMachine의 조건부 전환으로 자동 처리됨)
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (p_context.PlayerMovement != null && p_context.PlayerController.MoveInput != Vector2.zero)
        {
            // 2D 입력을 3D 월드 좌표로 변환
            Vector3 moveDirection = new Vector3(p_context.PlayerController.MoveInput.x, 0, p_context.PlayerController.MoveInput.y);
            p_context.PlayerMovement.Move(moveDirection, p_context.PlayerMovement.MoveSpeed);
        }
    }

    public override void OnExit()
    {
        p_context.PlayerAnimator.SetBool("IsMoving", false);

        Debug.Log("Player exited Move state");
    }
}