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
        // 대기 상태에서는 움직이지 않음
        p_context.Movement?.Move(Vector3.zero, 0f, 0f);
    }

    public override void OnExit()
    {
        p_context.Animator.SetBool("IsIdle", false);
    }
}