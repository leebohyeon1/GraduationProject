using UnityEngine;
using BehaviorTree;
using Pathfinding;

[CreateAssetMenu(fileName = "ChasePlayerNode", menuName = "BehaviorTree/ChasePlayerNode")]
public class ChasePlayerNode : Node
{

    public override void OnEnter()
    {
        runner.GetComponent<AIPath>().enableRotation = true;
        
        runner.SetState(Enemy.EnemyState.Chase);
    }

    protected override NodeState OnUpdate()
    {
        if (runner.player == null)
        {
            return NodeState.FAILURE;
        }
        runner.Movement.StartOrUpdateChase(runner.player.transform, "Run");
        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        
        runner.Movement.StopMovement();
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}