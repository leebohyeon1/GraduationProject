using UnityEngine;
using BehaviorTree;

public class Condition_BlackboardEnumKeyCheck : ConditionNode
{
    [Tooltip("enum 비교용")]
    public StateCondition stateCondition; // 이전에 만든 enum 조건
    protected override bool CheckCondition()
    {
        if (exceptCondition!=null && exceptCondition.isCondition(brain.blackboard))
        {
            return true;
        }
        bool keyValue = stateCondition.isCondition(brain.blackboard);
        // // // Debug.Log(string.Format("[BT] Condition Check: {0} ({1}) - Key: {2}, Result: {3}", this.name, this.GetType().Name, stateCondition.Key, keyValue));
        return keyValue;
    }
    public override Node Clone()
    {
        Condition_BlackboardEnumKeyCheck node = ScriptableObject.CreateInstance<Condition_BlackboardEnumKeyCheck>();
        node.stateCondition = this.stateCondition;
        return node;
    }
}