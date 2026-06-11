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
        
        if (ai != null)
        {
            ai.canMove = true;
            ai.isStopped = false;
            ai.maxSpeed = MoveSpeed;
            ai.enableRotation = false;
            ai.SetPath(null); 
        }
        runner._stateController.SetLock(false);

    }
    protected override NodeState OnUpdate()
    {

        if(runner._animationBridge.IsAttacking)
        {
            return NodeState.RUNNING; 
        }   

        if(runner.CurrentState == EnemyStateController.EnemyState.Attack || runner.CurrentState == EnemyStateController.EnemyState.Hit)
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

        
        // Debug.Log($"[Task_Pressure] {runner.name} is moving towards {targetPos}.");
        RotateTowardsPlayer();
        runner.Movement.UpdateStrafeAnim();
        runner.Movement.StartOrUpdateChase(targetPos, EnemyStateController.EnemyState.Chase, MoveSpeed);

        return NodeState.RUNNING;
    }
    public override void Abort()
    {
        base.Abort();
    }
    public override void OnExit()
    {
        base.OnExit();
        runner.Movement.StopMovement();
        runner._stateController.SetLock(false);
    }
    private void RotateTowardsPlayer()
    {
        if (runner.player == null) return;
        ai.enableRotation = false;
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
