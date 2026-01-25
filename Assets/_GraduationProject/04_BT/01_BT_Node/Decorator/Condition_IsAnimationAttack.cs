using UnityEngine;
using BehaviorTree;
public class Condition_IsAnimationAttack : ConditionNode
{
    public string tagstring = "Attack";
    protected override bool CheckCondition()
    {
        return !runner._animationBridge.IsAttacking;
    }
}