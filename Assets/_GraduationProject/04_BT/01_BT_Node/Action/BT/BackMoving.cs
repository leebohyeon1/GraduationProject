using UnityEngine;
using BehaviorTree;
using Pathfinding;
public class BackMoving : Node
{
    [Tooltip("유지하고 싶은 목표 거리입니다.")]
    public float targetDistance = 5.0f;

    [Tooltip("이동을 멈출 목표 지점과의 허용 오차 거리입니다.")]
    public float acceptanceRadius = 0.5f;

    [Tooltip("도망칠 때의 이동 속도입니다.")]
    public float runSpeed = 5;

    // 내부 변수
    private Transform playerTransform;
    private AIPath aiPath;

    // 이 노드가 활성화되어 있는 동안 절대 변하지 않을 고정된 목표 지점
    private Vector3 fixedTargetPosition;
    public float timeout = 1.8f;
    float startTime;

    public override void OnEnter()
    {
        playerTransform = runner.player.transform;
        aiPath = runner.GetComponent<AIPath>();

        if (aiPath == null)
        {
            Debug.LogError("AIPath 컴포넌트를 찾을 수 없습니다!", runner);
            return;
        }

        runner.SetState(Enemy.EnemyState.RunAway);

        aiPath.enableRotation = false; // 회전은 수동으로 제어

        // --- 핵심 로직 ---
        // 1. "노드 시작 시점"의 플레이어 위치를 단 한 번만 변수에 저장합니다.
        Vector3 initialPlayerPosition = playerTransform.position;

        // 2. 저장된 "최초 위치"를 기준으로 도망갈 고정된 목표 지점을 계산합니다.
        Vector3 directionFromInitialPlayer = (runner.transform.position - initialPlayerPosition);
        directionFromInitialPlayer.y = 0;

        // 이 노드가 끝날 때까지 목적지는 이 값으로 고정됩니다.
        fixedTargetPosition = runner.transform.position + directionFromInitialPlayer.normalized * targetDistance;


        runner.Movement.StartOrUpdateChase(fixedTargetPosition);
        startTime = Time.time;
    }

    protected override NodeState OnUpdate()
    {
        if (playerTransform == null || aiPath == null)
        {
            return NodeState.FAILURE;
        }

        // 회전: 이동과 별개로, "현재" 플레이어의 위치를 계속 바라봅니다.
        Vector3 currentDirectionToPlayer = (playerTransform.position - runner.transform.position);
        currentDirectionToPlayer.y = 0;

        if (currentDirectionToPlayer != Vector3.zero)
        {
            runner.transform.rotation = Quaternion.LookRotation(currentDirectionToPlayer);
        }
        RaycastHit hit;
        bool isHit = Physics.Raycast(
    runner.transform.position,       // 시작 위치
    -runner.transform.forward,       // 진행 방향 (여기선 뒤쪽)
    out hit,                         // 맞은 정보
    1f,                              // 거리
    LayerMask.GetMask("Wall")        // 레이어 마스크
);
        if (isHit)
        {
            Debug.Log("벽에 부딪혔습니다");
            return NodeState.SUCCESS;
        }
        if(Time.time - startTime > timeout)
        {
            Debug.Log("시간초과");
            return NodeState.SUCCESS;
        }
        // 도착 체크: AI가 "고정된 목표 지점"에 도착했는지 확인합니다.
        if (aiPath.reachedDestination)
        {
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    // Cleanup 로직은 이전과 동일합니다.
    private void Cleanup()
    {
        if (runner != null && aiPath != null)
        {
            // runner.Movement.StopMovement();
            runner.SetState(Enemy.EnemyState.Idle);
        }
    }

    public override void OnExit() { Cleanup(); }
    public override void Abort() { Cleanup(); }

    public override Node Clone()
    {
        BackMoving newNode = Instantiate(this);
        newNode.targetDistance = this.targetDistance;
        newNode.acceptanceRadius = this.acceptanceRadius;
        newNode.runSpeed = this.runSpeed;
        return newNode;
    }
}

