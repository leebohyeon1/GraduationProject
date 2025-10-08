using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 이동 상태입니다.
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
        HandleMovement();
    }

    /// <summary>
    /// 이동 입력을 처리합니다.
    /// </summary>
    private void HandleMovement()
    {
        if (p_context.Movement != null && p_context.Input.MoveInput != Vector2.zero)
        {
            Vector3 moveDirection = new Vector3(p_context.Input.MoveInput.x, 0, p_context.Input.MoveInput.y).normalized;
            p_context.Movement.Move(moveDirection, p_context.Stats.MoveSpeed, p_context.Stats.RotateSpeed);
        }
    }

    public override void OnExit()
    {
        p_context.Animator.SetBool("IsMoving", false);
    }
}