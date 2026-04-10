using UnityEngine;
using BehaviorTree;

public class Condition_BlackboardKeyCheck : ConditionNode
{
    public string key = "";

    protected override bool CheckCondition()
    {
        
        if (exceptCondition!=null && exceptCondition.isCondition(brain.blackboard))
        {
            return true;
        }
        bool keyValue = brain.blackboard.GetValue<bool>(key);
        return keyValue;
    }
    public override Node Clone()
    {
        Condition_BlackboardKeyCheck node = ScriptableObject.CreateInstance<Condition_BlackboardKeyCheck>();
        node.key = this.key;
        return node;
    }
}