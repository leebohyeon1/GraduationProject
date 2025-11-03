using UnityEngine;
using BehaviorTree;
public class Condition_IsOverHeat : ConditionNode
{
    public override Node Clone()
    {
        return Instantiate(this);
    }

    protected override bool CheckCondition()
    {
        return runner.heatSystem.IsOverHeat;
    }
}