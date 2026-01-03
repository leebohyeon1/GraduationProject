using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "ChasePlayerNode", menuName = "BehaviorTree/ChasePlayerNode")]
public class ChasePlayerNode : Node
{
    AIPath aIPath;
    public float Distance = 3;
    public override void OnEnter()
    {
        aIPath = runner.GetComponent<AIPath>();
        runner.SetState(Enemy.EnemyState.Chase);
        aIPath.enableRotation = false;

    }
    protected override NodeState OnUpdate()
    {
        if (runner.player == null)
        {
            return NodeState.FAILURE;
        }
        runner.Movement.StartOrUpdateChase(runner.player.transform);
        float distance = Vector3.Distance(runner.transform.position, runner.player.transform.position);
        if(distance <= Distance)
        {
            return NodeState.SUCCESS;
            
        }
        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        
        runner.Movement.StopMovement();
        aIPath.enableRotation = true;
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}