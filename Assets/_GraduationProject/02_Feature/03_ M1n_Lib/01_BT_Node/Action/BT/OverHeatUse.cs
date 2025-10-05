using UnityEngine;
using BehaviorTree;

public class OverHeatUse : Node
{
    public override Node Clone()
    {
        return Instantiate(this);
    }
    public override void OnEnter()
    {
        runner.heatSystem.OverHeatUse();
    }
    protected override NodeState OnUpdate()
    {
        return NodeState.SUCCESS;
    }
}