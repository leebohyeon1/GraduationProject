using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "Action_ClearStun", menuName = "BehaviorTree/Action/ClearStun")]
public class Action_ClearStun : ActionNode
{
    protected override NodeState OnUpdate()
    {
        if (runner == null || runner.ParrySystem == null) return NodeState.FAILURE;
        
        runner.ParrySystem.ClearStun();
        return NodeState.SUCCESS;
    }

    public override Node Clone() => Instantiate(this);
}
