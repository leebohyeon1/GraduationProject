using UnityEngine;
using BehaviorTree;

public class Condition_IsAllyNotCalled : ConditionNode
{
    protected override bool CheckCondition()
    {
        return runner != null && !brain._isCombat;
    }
    public override Node Clone() => Instantiate(this);

}