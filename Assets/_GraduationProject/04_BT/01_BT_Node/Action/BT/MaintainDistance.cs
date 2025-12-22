using UnityEngine;
using Pathfinding; // A* Pathfinding Project 네임스페이스 추가

using BehaviorTree;

[CreateAssetMenu(fileName = "MaintainDistance", menuName = "BehaviorTree/MaintainDistance")]
public class MaintainDistance : Node
{
    [Tooltip("유지하고 싶은 목표 거리입니다.")]
    public float targetDistance = 5.0f;

    [Tooltip("이동할 때 플레이어의 예상 위치를 얼마나 앞서서 계산할지 결정합니다.")]
    public float predictionTime = 0.5f;

    [Tooltip("이동을 멈출 목표 지점과의 허용 오차 거리입니다.")]
    public float acceptanceRadius = 0.5f;

    // 내부 변수
    private Transform playerTransform;
    private AIPath aiPath;


    public override void OnEnter()
    {
        playerTransform = runner.player.transform;
        aiPath = runner.GetComponent<AIPath>();

        if (aiPath == null)
        {
            Debug.LogError("AIPath 컴포넌트를 찾을 수 없습니다!", runner);
        }
        runner.SetState(Enemy.EnemyState.RunAway);
        aiPath.enableRotation = false;
    }

    protected override NodeState OnUpdate()
    {
        if (playerTransform == null || aiPath == null)
        {
            return NodeState.FAILURE;
        }
        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            runner.transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
        Vector3 vectorToPlayer = runner.transform.position - playerTransform.position;
        float currentDistance = vectorToPlayer.magnitude;

        // 목표 지점 계산
        Vector3 targetPosition;
        if (currentDistance < targetDistance) // 너무 가까우면
        {
            // 플레이어로부터 멀어지는 방향으로 목표 지점 설정
            targetPosition = runner.transform.position + vectorToPlayer.normalized * (targetDistance - currentDistance);
        }
        else // 너무 멀면
        {
            // 플레이어의 예상 이동 위치를 향해 다가감
            // 간단한 예측. 실제 플레이어의 Rigidbody 속도를 사용하면 더 정확합니다.
            Vector3 predictedPlayerPosition = playerTransform.position; // 여기에 플레이어 이동 예측 로직 추가 가능
            targetPosition = predictedPlayerPosition + (runner.transform.position - predictedPlayerPosition).normalized * targetDistance;
        }

        // A* 에이전트의 목표 지점 설정
        runner.Movement.StartOrUpdateChase(targetPosition);
    

        // 목표 지점에 충분히 가까워졌는지 확인
        if (aiPath.reachedDestination || aiPath.remainingDistance <= acceptanceRadius)
        {
            runner.Movement.StopMovement();
            return NodeState.SUCCESS;

        }

        // 아직 이동 중이면 RUNNING 상태 반환
        return NodeState.RUNNING;
    }

    public override Node Clone()
    {
        MaintainDistance newNode = Instantiate(this);
        newNode.targetDistance = this.targetDistance;
        newNode.predictionTime = this.predictionTime;
        newNode.acceptanceRadius = this.acceptanceRadius;

        return newNode;
    }

    public override void OnExit()
    {
        runner.Movement.StopMovement();
        aiPath.enableRotation = true;
        runner.SetState(Enemy.EnemyState.Idle);
    }
    public override void Abort()
    {
        runner.Movement.StopMovement();
        aiPath.enableRotation = true;
        runner.SetState(Enemy.EnemyState.Idle);
    }
}
