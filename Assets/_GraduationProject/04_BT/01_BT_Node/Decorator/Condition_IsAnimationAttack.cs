using UnityEngine;
using BehaviorTree;
public class Condition_IsAnimationAttack : ConditionNode
{
    public string tagstring = "Attack";
    protected override bool CheckCondition()
    {
        return !runner.animator.GetCurrentAnimatorStateInfo(0).IsTag(tagstring);
    }
}