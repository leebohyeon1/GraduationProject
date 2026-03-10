using UnityEngine;
using BehaviorTree;

/// <summary>
/// 피격(Hit) 애니메이션이 재생되는 동안 BT의 다른 노드 실행을 차단하고, 
/// 애니메이션이 끝나면 플래그를 정리하는 노드입니다.
/// </summary>
[CreateAssetMenu(fileName = "Task_HitAction", menuName = "BehaviorTree/Action/HitAction")]
public class Task_HitAction : Node
{
    private float _entryTime;
    private int _entryFrame;

    public override void OnEnter()
    {
        base.OnEnter();
        _entryTime = Time.time;
        _entryFrame = Time.frameCount;

        // 1. 애니메이션 신호 초기화 (이전 행동의 잔상 제거)
        if (Handler != null) Handler.ResetAllFlags();
        
        // 2. 상태를 Hit으로 확실히 설정
        runner.SetState(EnemyStateController.EnemyState.Hit);
        
        // 3. 이동 정지
        if (runner.Movement != null) runner.Movement.StopMovement();
        
        // // Debug.Log($"<color=orange>[Task_HitAction]</color> 히트 대기 시작 (ID: {runner.name})");
    }

    protected override NodeState OnUpdate()
    {
        if (runner == null) return NodeState.FAILURE;

        // 애니메이터 상태 갱신을 위한 최소 프레임 대기
        if (Time.frameCount <= _entryFrame + 1) return NodeState.RUNNING;

        // 1. 애니메이션 종료 이벤트(FinishAction) 감지
        if (Handler != null && Handler.IsActionFinished)
        {
            // // Debug.Log("<color=orange>[Task_HitAction]</color> 히트 애니메이션 종료 감지.");
            return NodeState.SUCCESS;
        }

        // 2. 안전 타임아웃 (애니메이션 이벤트 누락 대비, 보통 1초면 충분)
        if (Time.time - _entryTime > 1.2f)
        {
            // // Debug.LogWarning("<color=orange>[Task_HitAction]</color> 히트 대기 타임아웃.");
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        base.OnExit();
        
        // 1. 피격 플래그 해제 (매우 중요: 다음 선택 로직이 작동할 수 있게 함)
        brain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, false);
        
        // 2. 상태를 Idle로 복구
        if (runner.CurrentState == EnemyStateController.EnemyState.Hit)
        {
            runner.SetState(EnemyStateController.EnemyState.Idle);
        }

        if (Handler != null) Handler.ResetAllFlags();
        
        // // Debug.Log("<color=orange>[Task_HitAction]</color> 히트 상태 해제 및 플래그 초기화.");
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}
