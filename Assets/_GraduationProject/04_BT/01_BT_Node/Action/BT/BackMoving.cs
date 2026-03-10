using UnityEngine;
using BehaviorTree;
using Pathfinding;

public class BackMoving : Node
{
    [Tooltip("유지하고 싶은 목표 거리입니다.")]
    public float targetDistance = 5.0f;

    [Tooltip("이동을 멈출 목표 지점과의 허용 오차 거리입니다.")]
    public float acceptanceRadius = 0.5f;

    // 내부 변수
    private Transform playerTransform;
    private AIPath aiPath;

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

        runner.SetState(EnemyStateController.EnemyState.RunAway);
        aiPath.enableRotation = false; // 회전은 수동으로 제어
        
        startTime = Time.time;
    }

    protected override NodeState OnUpdate()
    {
        if (playerTransform == null || aiPath == null)
        {
            return NodeState.FAILURE;
        }

        Vector3 playerFacingDir = playerTransform.forward;
        playerFacingDir.y = 0; // 높낮이 무시
        playerFacingDir.Normalize();

        Vector3 currentTargetPosition = runner.transform.position + (playerFacingDir * targetDistance);

        runner.Movement.StartOrUpdateChase(currentTargetPosition);


        Vector3 currentDirectionToPlayer = (playerTransform.position - runner.transform.position);
        currentDirectionToPlayer.y = 0;

        if (currentDirectionToPlayer != Vector3.zero)
        {
            runner.transform.rotation = Quaternion.LookRotation(currentDirectionToPlayer);
        }

        RaycastHit hit;
        bool isHit = Physics.Raycast(
            runner.transform.position,       
            -runner.transform.forward,      
            out hit,                         
            1f,                              
            LayerMask.GetMask("Wall")        
        );

        if (isHit)
        {
            // Debug.Log("벽에 부딪혔습니다");
            return NodeState.SUCCESS;
        }

        // --- 종료 조건 ---
        if (Time.time - startTime > timeout)
        {
            // Debug.Log("시간초과");
            return NodeState.SUCCESS;
        }

        if (aiPath.reachedDestination)
        {
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    private void Cleanup()
    {
        if (runner != null && aiPath != null)
        {
            runner.Movement.StopMovement(); 
            runner.SetState(EnemyStateController.EnemyState.Idle);
        }
    }

    public override void OnExit() { Cleanup(); }
    public override void Abort() { Cleanup(); }

    public override Node Clone()
    {
        BackMoving newNode = Instantiate(this);
        newNode.targetDistance = this.targetDistance;
        newNode.acceptanceRadius = this.acceptanceRadius;
        return newNode;
    }
}