// RushToPlayer.cs 파일
using UnityEngine;
using BehaviorTree;

public class RushToPlayer : Node
{
    [SerializeField] float _rushSpeed = 20f;
    [SerializeField] float _successDistance = 1.5f;
    [SerializeField] float _timeout = 5.0f;

    private float _startTime;
    private Vector3 _targetPosition;

    public override void OnEnter()
    {
        // // BTDebug.Log("<color=green>--RUSH--: OnEnter</color>");
        // _startTime = Time.time;
        
        _targetPosition = runner.player.transform.position;
        _targetPosition.y = runner.transform.position.y;
        
        runner.SetState(EnemyStateController.EnemyState.Rush);
        runner.Movement.StartRush(_targetPosition, _rushSpeed);
    }

    protected override NodeState OnUpdate()
    {
        // OnUpdate는 매 프레임 호출되므로 로그는 필요 시에만 활성화
        // // // BTDebug.Log("--RUSH--: OnUpdate");

        // if (Time.time - _startTime > _timeout)
        // {
        //     Debug.LogError("<color=red>--RUSH--: TIMEOUT! AI did not reach the target in time.</color>");
        //     return NodeState.FAILURE;
        // }

        float distanceToTarget = Vector3.Distance(runner.transform.position, _targetPosition);
        if (distanceToTarget <= _successDistance)
        {
            // // BTDebug.Log("<color=green>--RUSH--: SUCCESS! Target reached.</color>");
            return NodeState.SUCCESS;
        }
        
        return NodeState.RUNNING;
    }

    public override void OnExit()
{
    // // BTDebug.Log("<color=green>--RUSH--: OnExit</color>");

    // StopMovement()를 호출하기 전에, 자신의 상태를 먼저 변경하여 
    // StopMovement()의 보호 로직을 정상적으로 통과할 수 있게 합니다.
    if (runner.CurrentState == EnemyStateController.EnemyState.Rush)
    {
        runner.SetState(EnemyStateController.EnemyState.Idle);
    }
    // ★★★ 여기까지 추가 ★★★

    runner.Movement.StopMovement();
}
    
    public override Node Clone()
    {
        RushToPlayer clone = Instantiate(this);
        clone._rushSpeed = _rushSpeed;
        clone._successDistance = _successDistance;
        clone._timeout = _timeout;
        return clone;
    }
}