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
        
        Debug.Log(string.Format("[Task_Pressure : {0}] OnEnter 진입. 현재 상태: {1}, Lock: {2}", runner.name, runner.CurrentState, runner._stateController.IsStateLocked));

        if (ai != null)
        {
            ai.canMove = true;
            ai.isStopped = false;
            ai.maxSpeed = MoveSpeed;
            ai.enableRotation = false;
        }
    }
    protected override NodeState OnUpdate()
    {
        if(runner._animationBridge.IsAttacking)
        {
            Debug.Log(string.Format("[Task_Pressure : {0}] OnUpdate 실패: 애니메이션 브릿지가 공격 중임.", runner.name));
            return NodeState.FAILURE;
        }   
        if(runner.CurrentState == EnemyStateController.EnemyState.Attack)
        {
            Debug.Log(string.Format("[Task_Pressure : {0}] OnUpdate 실패: 현재 상태가 Attack임.", runner.name));
            return NodeState.FAILURE;
        }
        
        object val = brain.blackboard.GetValue<Vector3>(Pos_Key);
        if (val == null)
        {
            Debug.LogWarning(string.Format("[Task_Pressure : {0}] OnUpdate 실패: 블랙보드 키 '{1}'가 비어있음.", runner.name, Pos_Key));
            return NodeState.FAILURE;
        }

        Vector3 targetPos = (Vector3)val;
        Debug.Log(string.Format("[Task_Pressure : {0}] targetPos from blackboard: {1}", runner.name, targetPos));
        currentTargetDebug = targetPos; 
        
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
        var node = Instantiate(this);
        node.Pos_Key = this.Pos_Key;
        node.MoveSpeed = this.MoveSpeed;
        node.StoppingDist = this.StoppingDist;
        node.RotationSpeed = this.RotationSpeed;
        return node;
    }
}
