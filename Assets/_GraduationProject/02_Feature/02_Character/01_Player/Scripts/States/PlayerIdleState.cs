using BH_Lib.FSM;
using UnityEngine;

/// <summary>
/// 플레이어 대기 상태
/// 입력이 없을 때의 기본 상태
/// </summary>
public class PlayerIdleState : BaseState<Player>
{
    public PlayerIdleState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine)
    {
    }

    public override void OnEnter()
    {
        // 대기 상태 진입 시 처리
        Debug.Log("Player entered Idle state");
    }

    public override void OnUpdate()
    {
        // 상태 전환은 StateMachine의 조건부 전환으로 자동 처리됨
        // 필요한 경우 여기서 Idle 상태 특유의 로직 구현
    }

    public override void OnExit()
    {
        Debug.Log("Player exited Idle state");
    }
}