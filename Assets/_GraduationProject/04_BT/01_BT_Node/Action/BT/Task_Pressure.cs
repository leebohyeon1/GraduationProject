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
    private Vector3? currentTargetDebug; 

    public override void OnEnter()
    {
        base.OnEnter();
        ai = runner.aIPath;
        
        Debug.Log(string.Format("[Task_Pressure : {0}] OnEnter 진입. 현재 상태: {1}, Lock: {2}, AnimAtk: {3}", 
            runner.name, runner.CurrentState, runner._stateController.IsStateLocked, runner._animationBridge.IsAttacking));

        if (ai != null)
        {
            ai.canMove = true;
            ai.isStopped = false;
            ai.maxSpeed = MoveSpeed;
            ai.enableRotation = false;
            ai.SetPath(null); // 진입 시 잔여 경로 제거
        }
    }
    protected override NodeState OnUpdate()
    {
        // [수정] 애니메이션 브릿지의 IsAttacking이 true더라도, 현재 상태가 Attack이 아니면 이동 허용 고려
        // 하지만 안전을 위해 로그를 남기고 실패 처리 유지 (BaseAttackNode에서 강제 해제하므로 이제 발생 안 함)
        if(runner._animationBridge.IsAttacking)
        {
            // Debug.Log(string.Format("[Task_Pressure : {0}] OnUpdate 대기: 애니메이션 브릿지가 아직 공격 중임.", runner.name));
            return NodeState.RUNNING; // 실패 대신 대기하여 트리가 튀지 않게 함
        }   

        if(runner.CurrentState == EnemyStateController.EnemyState.Attack)
        {
            return NodeState.FAILURE;
        }
        
        object val = brain.blackboard.GetValue<Vector3>(Pos_Key);
        if (val == null)
        {
            return NodeState.FAILURE;
        }

        Vector3 targetPos = (Vector3)val;
        currentTargetDebug = targetPos; 
        
        // A* 경로 업데이트 강제
        runner.Movement.StartOrUpdateChase(targetPos, EnemyStateController.EnemyState.Chase, MoveSpeed);

        RotateTowardsPlayer();
        runner.Movement.UpdateStrafeAnim();

        return NodeState.RUNNING;
    }
    public override void Abort()
    {
        base.Abort();
        Debug.Log(string.Format("[Task_Pressure : {0}] Abort 호출됨.", runner.name));
    }
    public override void OnExit()
    {
        base.OnExit();
        Debug.Log(string.Format("[Task_Pressure : {0}] OnExit 호출됨.", runner.name));
        if (ai != null) ai.enableRotation = true;
    }
    private void RotateTowardsPlayer()
    {
        if (runner.player == null) return;

        Vector3 directionToPlayer = runner.player.transform.position - runner.transform.position;
        directionToPlayer.y = 0; 

        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            runner.transform.rotation = Quaternion.Slerp(runner.transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
    }

    public void OnDrawGizmos()
    {
        if (runner != null && currentTargetDebug.HasValue)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentTargetDebug.Value, 0.3f); 
            Gizmos.DrawLine(runner.transform.position, currentTargetDebug.Value); 
        }
    }
    public override Node Clone()
    {
        var node = Instantiate(this); // [수정] CreateInstance 대신 Instantiate 사용 (SO 복제 표준)
        node.Pos_Key = this.Pos_Key;
        node.MoveSpeed = this.MoveSpeed;
        node.StoppingDist = this.StoppingDist;
        node.RotationSpeed = this.RotationSpeed;
        return node;
    }
}
