using BehaviorTree;
using UnityEngine;

public class Condition_IsActionable : ConditionNode
{
    protected override bool CheckCondition()
    {
        return runner != null && !brain.IsActionable();
    }

    public override Node Clone() => Instantiate(this);
}