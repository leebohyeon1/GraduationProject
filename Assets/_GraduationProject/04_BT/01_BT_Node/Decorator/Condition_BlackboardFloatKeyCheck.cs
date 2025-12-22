using UnityEngine;
using BehaviorTree;

public class Condition_BlackboardFloatKeyCheck : ConditionNode
{
    [Tooltip("float 비교용")]
    public FloatCondition floatCondition; // 이전에 만든 float 조건
    public ExceptCondition exceptCondition; // 제외 조건
    protected override bool CheckCondition()
    {
        
        if (exceptCondition!=null && exceptCondition.isCondition(brain.blackboard))
        {
            return true;
        }

        bool keyValue = floatCondition.isCondition(brain.blackboard);
        return keyValue;
    }
    public override Node Clone()
    {
        Condition_BlackboardFloatKeyCheck node = ScriptableObject.CreateInstance<Condition_BlackboardFloatKeyCheck>();
        node.floatCondition = this.floatCondition;
        return node;
    }
}