using UnityEngine;
using BehaviorTree;
using Pathfinding;
using Pathfinding.RVO;

public class Task_Pressure : Node
{
    public string Pos_Key = "PressurePos";
    public float MoveSpeed = 4.0f;
    public float StoppingDist = 0.5f;
    public float RotationSpeed = 5.0f;
    private AIPath ai;
    private Vector3? currentTargetDebug; // 디버그용 그림 그리기 변수

    public override void OnEnter()
    {
        base.OnEnter();
        ai = runner.aIPath;
        ai.enableRotation = false;
        // Debug.Log($"[Action_Enter] {runner.name} 압박 이동 노드 진입됨.");
    }
    protected override NodeState OnUpdate()
    {
        if(runner._animationBridge.IsAttacking)
        {
            // Debug.Log($"[{runner.name} 공격애니메.");
            return NodeState.FAILURE;
        }   
        if(runner.CurrentState == EnemyStateController.EnemyState.Attack)
        {
            // Debug.Log($"[{runner.name} 공격스테이트.");
            return NodeState.FAILURE;
        }
        
        // 1. 블랙보드 값 확인
        object val = brain.blackboard.GetValue<Vector3>(Pos_Key);
        if (val == null)
        {
            Debug.LogWarning($"[Action_Warning] 블랙보드 키 '{Pos_Key}'가 비어있습니다. Service가 돌고 있나요?");
            return NodeState.FAILURE;
        }

        Vector3 targetPos = (Vector3)val;
        currentTargetDebug = targetPos; // 기즈모 그리기용 저장
        
        runner.Movement.StartOrUpdateChase(targetPos);

        RotateTowardsPlayer();
        runner.Movement.UpdateStrafeAnim();
        // 이동 상태 디버깅 (너무 많이 뜨면 주석 처리하세요)


        return NodeState.RUNNING;
    }
    public override void Abort()
    {
        base.Abort();
        // runner.Movement.StopMovement();
    }
    public override void OnExit()
    {
        base.OnExit();
        ai.enableRotation = true;
        // runner.Movement.StopMovement();
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