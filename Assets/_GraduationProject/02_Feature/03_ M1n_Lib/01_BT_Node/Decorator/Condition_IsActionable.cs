using BehaviorTree;
using UnityEngine;

[CreateAssetMenu(fileName = "IsActionable_Condition", menuName = "BehaviorTree/Condition/IsActionable")]
public class Condition_IsActionable : ConditionNode
{
    protected override bool CheckCondition()
    {
        return runner != null && !brain.IsActionable();
    }

    public override Node Clone() => Instantiate(this);
}