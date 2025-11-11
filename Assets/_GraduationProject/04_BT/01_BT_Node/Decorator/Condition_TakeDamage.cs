using BehaviorTree;
using UnityEngine;


public class Condition_TakeDamage : ConditionNode
{
    public override Node Clone()
    {
        Condition_TakeDamage node = CreateInstance<Condition_TakeDamage>();
        return node;
    }

    protected override bool CheckCondition()
    {
        return runner.CurrentState == Enemy.EnemyState.Hit;
    }


}
