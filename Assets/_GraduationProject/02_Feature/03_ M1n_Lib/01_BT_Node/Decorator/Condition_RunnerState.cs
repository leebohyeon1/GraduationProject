using BehaviorTree;
using UnityEngine;

public class Condition_RunnerState : ConditionNode
{
    public Enemy.EnemyState requiredState;

    protected override bool CheckCondition()
    {
        return runner.CurrentState != requiredState;
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.requiredState = this.requiredState;
        return node;
    }
}