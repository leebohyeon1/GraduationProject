using BehaviorTree;
using UnityEngine;

public class PatrolTargetNode : Node
{
    public override Node Clone()
    {
        PatrolTargetNode newNode = Instantiate(this);
        return newNode;
    }
    public override void OnEnter()
    {
        
    }
    protected override NodeState OnUpdate()
    {
        if (brain._isCombat)
        {
            return NodeState.FAILURE;
        }
        if(brain.blackboard.GetValue<bool>("IsPlayerDetected", out bool DetectPlayer) && DetectPlayer)
        {
            return NodeState.FAILURE;
        }
        runner.Movement.StartOrUpdateChase(runner.player.transform.position, EnemyStateController.EnemyState.Patrol);

        return NodeState.SUCCESS;
    }
}