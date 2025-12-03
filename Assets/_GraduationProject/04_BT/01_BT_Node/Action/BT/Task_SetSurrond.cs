
using UnityEngine;
using BehaviorTree;

public class Task_SetSurrond : Node
{
    public override Node Clone()
    {
        return Instantiate(this);
    }
    public override void OnEnter()
    {
        runner.groupAi.AssignSlots();
    }
    protected override NodeState OnUpdate()
    {
        return NodeState.SUCCESS;
    }
}