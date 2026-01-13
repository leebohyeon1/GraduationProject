using UnityEngine;
using BehaviorTree;
using Pathfinding;

public class Task_Pressure : Node
{
    public string Pos_Key = "PressurePos";
    public float MoveSpeed = 4.0f;
    public float StoppingDist = 0.5f;
    public float RotationSpeed = 5.0f;
    private IAstarAI aiAgent;
    private Vector3? currentTargetDebug; // 디버그용 그림 그리기 변수
    public override void OnEnter()
    {
        base.OnEnter();
        aiAgent = runner.GetComponent<IAstarAI>();
        

        if (aiAgent == null)
        {
            return;
        }

        aiAgent.maxSpeed = MoveSpeed;
        if (aiAgent is AIPath aiPath) {
            aiPath.endReachedDistance = StoppingDist;
            aiPath.enableRotation = false;
        }
    }

    protected override NodeState OnUpdate()
    {
        if(runner.CurrentState == EnemyStateController.EnemyState.Attack)
        {
            return NodeState.FAILURE;
        }
        if(runner.animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            return NodeState.FAILURE;
        }   
        if (aiAgent == null) return NodeState.FAILURE;
        
        // 1. 블랙보드 값 확인
        object val = brain.blackboard.GetValue<Vector3>(Pos_Key);
        if (val == null)
        {
            Debug.LogWarning($"[Action_Warning] 블랙보드 키 '{Pos_Key}'가 비어있습니다. Service가 돌고 있나요?");
            return NodeState.FAILURE;
        }

        Vector3 targetPos = (Vector3)val;
        currentTargetDebug = targetPos; // 기즈모 그리기용 저장

        // 2. 목적지 설정 및 로그
        // aiAgent.destination = targetPos;
        runner.Movement.StartOrUpdateChase(targetPos, EnemyStateController.EnemyState.Chase, MoveSpeed);
        RotateTowardsPlayer();
        
        // 이동 상태 디버깅 (너무 많이 뜨면 주석 처리하세요)
        // Debug.Log($"[Action_Run] 목표: {targetPos} | 현재: {runner.transform.position} | 남은거리: {Vector3.Distance(runner.transform.position, targetPos)}");

        // 3. 도착 확인 로그
        if (aiAgent.reachedEndOfPath)
        {
            Debug.Log($"[Action_Success] 도착 완료! (Pos: {targetPos})");
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }
    public override void Abort()
    {
        base.Abort();
        Debug.Log($"[Action_Abort] {runner.name} 압박 이동 노드 중단됨.");
        runner.Movement.StopMovement();
    }
    private void RotateTowardsPlayer()
    {
        if (runner.player == null) return;

        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0; // 위아래로 기울지 않도록 Y축 제거

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            // Slerp를 사용하여 부드럽게 회전
            runner.transform.rotation = Quaternion.Slerp(runner.transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
    }

    // Scene 뷰에 목표 지점과 선을 그려주는 함수
    public void OnDrawGizmos()
    {
        if (runner != null && currentTargetDebug.HasValue)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentTargetDebug.Value, 0.3f); // 목표 지점 공
            Gizmos.DrawLine(runner.transform.position, currentTargetDebug.Value); // 내 위치 -> 목표 선
        }
    }
    public override Node Clone()
    {
        var node = ScriptableObject.CreateInstance<Task_Pressure>();
        node.Pos_Key = this.Pos_Key;
        node.MoveSpeed = this.MoveSpeed;
        node.StoppingDist = this.StoppingDist;
        return node;
    }
}