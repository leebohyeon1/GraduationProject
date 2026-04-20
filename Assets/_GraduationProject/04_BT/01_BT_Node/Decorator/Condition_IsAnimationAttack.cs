using UnityEngine;
using BehaviorTree;
public class Condition_IsAnimationAttack : ConditionNode
{
    protected override bool CheckCondition()
    {
        return !runner._animationBridge.IsAttacking;
    }
}