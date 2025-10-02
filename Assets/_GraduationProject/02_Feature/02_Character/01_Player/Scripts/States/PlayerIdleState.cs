using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 플레이어 대기 상태
/// 입력이 없을 때 기본 상태
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
        // Idle 상태에서의 중력 처리 (이동 입력 없음)
        p_context.Movement?.Move(Vector3.zero, 0f, 0f);
    }

    public override void OnExit()
    {
        p_context.Animator.SetBool("IsIdle", false);
    }
}