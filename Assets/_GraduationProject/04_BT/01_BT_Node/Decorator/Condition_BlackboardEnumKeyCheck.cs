using UnityEngine;
using BehaviorTree;

public class Condition_BlackboardEnumKeyCheck : ConditionNode
{
    [Tooltip("enum 비교 조건")]
    public StateCondition stateCondition; // ?댁쟾??留뚮뱺 enum 議곌굔
    protected override bool CheckCondition()
    {
        if (exceptCondition!=null && exceptCondition.isCondition(brain.blackboard))
        {
            return true;
        }
        bool keyValue = stateCondition.isCondition(brain.blackboard);
        return keyValue;
    }
    public override Node Clone()
    {
        Condition_BlackboardEnumKeyCheck node = ScriptableObject.CreateInstance<Condition_BlackboardEnumKeyCheck>();
        node.stateCondition = this.stateCondition;
        return node;
    }
}
