using UnityEngine;
using BehaviorTree;

public class Condition_IsAbility : ConditionNode
{
    protected override bool CheckCondition()
    {
        return runner.specialAbility.AbilityReady;
    }

    public override Node Clone()
    {
        return ScriptableObject.CreateInstance<Condition_IsAbility>();
    }
}