// 파일 경로: 02_Feature/03_ M1n_Lib/01_BT_Node/Action/BT/CircleBehindPlayerNode.cs

using UnityEngine;
using BehaviorTree;
using Pathfinding;

/// <summary>
/// EnemyMovement를 사용하여 플레이어 뒤로 원형 이동을 하는 노드 (플레이어 움직임 실시간 추적)
/// </summary>
[CreateAssetMenu(menuName = "BehaviorTree/Action/CircleBehindPlayerNode_Movement")]
public class CircleBehindPlayerNode : ActionNode
{
    [Tooltip("플레이어와 유지할 원형 이동의 거리(반지름)")]
    public float circleRadius = 10f;

    [Tooltip("최종 목표 지점에 얼마나 가까워져야 성공으로 처리할지")]
    public float stoppingDistance = 1.5f;

    private int circlingDirection = 1; // 1 for right, -1 for left
    AIPath _aiPath;
    bool _Target = false;
    public override void OnEnter()
    {
        if (runner.Movement == null)
        {
            Debug.LogError("EnemyMovement component is not available on the runner.");
            return;
        }
        _aiPath = runner.GetComponent<AIPath>();
        _aiPath.enableRotation = false;
        Transform playerTransform = runner.player.transform;
        Transform runnerTransform = runner.transform;

        // 1. 왼쪽으로 돌지, 오른쪽으로 돌지 최초 방향만 결정
        Vector3 toRunner = (runnerTransform.position - playerTransform.position).normalized;
        Vector3 playerBackDirection = -playerTransform.forward;
        float dotProduct = Vector3.Dot(Vector3.Cross(playerBackDirection, toRunner), Vector3.up);
        circlingDirection = (dotProduct > 0) ? 1 : -1;

        brain.SetState(Enemy.EnemyState.RunAway);

        
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0; // 수평면에서의 방향만 고려
        directionToPlayer.Normalize();

        Vector3 forward = runner.transform.forward;
        forward.y = 0; // 수평면에서의 방향만 고려
        forward.Normalize();

        float angle = Vector3.Angle(forward, directionToPlayer);


        if (angle < (180f - 50))
        {
            _Target = true;
        }
    }

    protected override NodeState OnUpdate()
    {
        if(_Target)
        {
            return NodeState.SUCCESS;
        }

        if (typeof(PlayerAttackBaseState).IsAssignableFrom(runner.player.CurrentPlayerState))
        {
            return NodeState.FAILURE;
        }
        if(runner.player.CurrentPlayerState == typeof(PlayerDodgeState))
        {
            return NodeState.FAILURE;
        }

        Transform playerTransform = runner.player.transform;
        Transform runnerTransform = runner.transform;

        // 2. 플레이어의 현재 위치를 기준으로 '최종 목표 지점'을 매 프레임 갱신
        Vector3 finalTargetPosition = playerTransform.position - playerTransform.forward * circleRadius;



        // 4. 갱신된 최종 목표에 근접했는지 확인
        if (Vector3.Distance(runnerTransform.position, finalTargetPosition) < stoppingDistance)
        {
            return NodeState.SUCCESS;
        }

        // ★★★ 수정된 핵심 로직 ★★★
        // 5. EnemyMovement를 사용하여 원형 경로의 다음 지점으로 이동 요청
        Vector3 directionToRunner = (runnerTransform.position - playerTransform.position).normalized;
        Vector3 tangentDirection = Vector3.Cross(directionToRunner, Vector3.up * -circlingDirection).normalized;

        Vector3 nextPointOnCircle = runnerTransform.position + tangentDirection;

        Vector3 directionFromPlayerToNextPoint = (nextPointOnCircle - playerTransform.position).normalized;
        Vector3 desiredNextPosition = playerTransform.position + directionFromPlayerToNextPoint * circleRadius;

        // EnemyMovement의 추적 함수 호출
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            runner.transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
        runner.Movement.StartOrUpdateChase(desiredNextPosition);

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        _aiPath.enableRotation = true;

        if (runner.Movement != null)
        {
            // EnemyMovement의 정지 함수 호출
            runner.Movement.StopMovement();
        }

        if (brain.CurrentState == Enemy.EnemyState.RunAway)
        {
            brain.SetState(Enemy.EnemyState.Idle);
        }
    }
    public override void Abort()
    {
        base.Abort();
        _aiPath.enableRotation = true;

    }
    public override Node Clone()
    {
        CircleBehindPlayerNode node = CreateInstance<CircleBehindPlayerNode>();
        node.circleRadius = circleRadius;
        node.stoppingDistance = stoppingDistance;
        return node;
    }
}