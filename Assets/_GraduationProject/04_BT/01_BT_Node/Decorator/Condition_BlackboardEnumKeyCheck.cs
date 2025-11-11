using UnityEngine;
using BehaviorTree;

public class Condition_BlackboardEnumKeyCheck : ConditionNode
{
    [Tooltip("enum 비교용")]
    public StateCondition stateCondition; // 이전에 만든 enum 조건
    protected override bool CheckCondition()
    {
        bool keyValue = stateCondition.isCondition(brain.blackboard);
        return keyValue;
    }
    public override Node Clone()
    {
        Condition_BlackboardEnumKeyCheck node = new Condition_BlackboardEnumKeyCheck();
        node.stateCondition = this.stateCondition;
        return node;
    }
}