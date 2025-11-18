using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 플레이어의 대기 상태입니다.
/// </summary>
public class PlayerIdleState : BaseState<Player>
{
    public PlayerIdleState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Animator.SetBool("IsIdle", true);
    }

    public override void OnFixedUpdate()
    {
        if (p_context.Stats.IsLockOn)
        {
            Vector3 targetPosition = new Vector3(p_context.LockOnSystem.CurrentTarget.position.x, 0, p_context.LockOnSystem.CurrentTarget.position.z);
            Vector3 directionToTarget = (targetPosition - new Vector3(p_context.transform.position.x, 0, p_context.transform.position.z)).normalized;

            p_context.Movement?.SetRotation(Quaternion.LookRotation(directionToTarget), p_context.Stats.Data.RotateSpeed);
        }

        // 대기 상태에서는 움직이지 않음
        p_context.Movement?.Move(Vector3.zero, 0f);
    }

    public override void OnExit()
    {
        p_context.Animator.SetBool("IsIdle", false);
    }
}