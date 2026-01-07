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
        p_context.Stats.AttackComboIndex = 0;
        p_context.Animator.SetInteger("ComboIndex", p_context.Stats.AttackComboIndex);
        p_context.Animator.SetBool("IsMoving", true);
    }

    public override void OnFixedUpdate()
    {
        if(p_context.InputDeviceDetector.CurrentInputDevice == InputDeviceType.Gamepad)
        {
            p_context.Animator.SetFloat("MoveInput", p_context.Input.MoveInput.magnitude);
            HandleMovement(p_context.Input.MoveInput.magnitude);
        }
        else
        {
            p_context.Animator.SetFloat("MoveInput", 1);
            HandleMovement();
        }
    }

    /// <summary>
    /// 이동 입력을 처리합니다.
    /// </summary>
    private void HandleMovement(float speedMagnification = 1)
    {
        if (p_context.Movement != null && p_context.Input.MoveInput != Vector2.zero)
        {
            Vector3 moveDirection = new Vector3(p_context.Input.MoveInput.x, 0, p_context.Input.MoveInput.y).normalized;

            if (p_context.Stats.IsLockOn)
            {
                Vector3 targetPosition = new Vector3(p_context.LockOnSystem.CurrentTarget.position.x, 0, p_context.LockOnSystem.CurrentTarget.position.z);
                Vector3 directionToTarget = (targetPosition - new Vector3(p_context.transform.position.x, 0, p_context.transform.position.z)).normalized;

                p_context.Movement?.SetRotation(Quaternion.LookRotation(directionToTarget), p_context.Stats.RuntimeData.RotateSpeed);
                p_context.Movement.Move(moveDirection, p_context.Stats.RuntimeData.MoveSpeed * speedMagnification);
            }
            else
            {
                p_context.Movement.Move(moveDirection, p_context.Stats.RuntimeData.MoveSpeed * speedMagnification, p_context.Stats.RuntimeData.RotateSpeed);
            }
        }
    }

    public override void OnExit()
    {
        p_context.Animator.SetBool("IsMoving", false);
    }
}