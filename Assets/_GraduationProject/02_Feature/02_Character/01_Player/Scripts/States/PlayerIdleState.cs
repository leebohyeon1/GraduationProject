using BH_Lib.FSM;
using UnityEngine;

/// <summary>
/// 플레이어 대기 상태
/// 입력이 없을 때의 기본 상태
/// </summary>
public class PlayerIdleState : BaseState<Player>
{
    public PlayerIdleState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) {}

    public override void OnEnter()
    {
        p_context.PlayerAnimator.SetBool("IsIdle", true);  
        // 대기 상태 진입 시 처리
        Debug.Log("Player entered Idle state");
    }

    public override void OnUpdate()
    {
        // Idle 상태에서도 중력 적용 (이동 입력 없이)
        p_context.PlayerMovement?.Move(Vector3.zero, 0f);
    }

    public override void OnExit()
    {
        p_context.PlayerAnimator.SetBool("IsIdle", false); 
        Debug.Log("Player exited Idle state");
    }
}