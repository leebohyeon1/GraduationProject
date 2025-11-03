using BehaviorTree;
using UnityEngine;

[CreateAssetMenu(fileName = "TakeDamage_Condition", menuName = "BehaviorTree/Condition/TakeDamage")]
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
