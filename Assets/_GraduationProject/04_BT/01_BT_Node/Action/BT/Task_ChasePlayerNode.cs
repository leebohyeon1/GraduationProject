using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "ChasePlayerNode", menuName = "BehaviorTree/ChasePlayerNode")]
public class Task_ChasePlayerNode : Node
{
    AIPath aIPath;
    public float Distance = 3;
    public float speed = 4;
    public override void OnEnter()
    {
        aIPath = runner.GetComponent<AIPath>();
        
        // Debug.Log(string.Format("[ChasePlayerNode : {0}] OnEnter 진입. 현재 상태: {1}, Lock: {2}", runner.name, runner.CurrentState, runner._stateController.IsStateLocked));

        if (aIPath != null)
        {
            aIPath.canMove = true;
            aIPath.isStopped = false;
            aIPath.maxSpeed = speed;
        }

        runner.SetState(EnemyStateController.EnemyState.Chase);
        runner.aIPath.enableRotation = true;
    }
    protected override NodeState OnUpdate()
    {
        if(runner._animationBridge.IsAttacking) {
            return NodeState.FAILURE;
        }
        if (runner.player == null)
        {
            return NodeState.FAILURE;
        }
        
        runner.Movement.StartOrUpdateChase(runner.player.transform.position, EnemyStateController.EnemyState.Chase, speed);
        
        float distance = Vector3.Distance(runner.transform.position, runner.player.transform.position);
        if(distance <= Distance)
        {
            return NodeState.SUCCESS;
        }
        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        // Debug.Log(string.Format("[ChasePlayerNode : {0}] OnExit 호출됨.", runner.name));
        runner.Movement.StopMovement();
    }
    public override void Abort()
    {
        base.Abort();
        runner.Movement.StopMovement();
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}
