
using UnityEngine;
using BehaviorTree;

public class Task_SetSurrond : Node
{
    public override Node Clone()
    {
        return Instantiate(this);
    }

    protected override NodeState OnUpdate()
    {
        return NodeState.SUCCESS;
    }
}