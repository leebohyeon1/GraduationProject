using UnityEngine;
using BehaviorTree;

public class Condition_BlackboardKeyCheck : ConditionNode
{


    public string key = "";

    protected override bool CheckCondition()
    {
        bool keyValue = brain.blackboard.GetValue<bool>(key);
        return keyValue;
    }
    public override Node Clone()
    {
        Condition_BlackboardKeyCheck node = new Condition_BlackboardKeyCheck();
        node.key = this.key;
        return node;
    }
}