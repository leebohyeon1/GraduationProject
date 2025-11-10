using UnityEngine;
using BehaviorTree;

public class Condition_BlackboardKeyCheck : ConditionNode
{
    public string key = "";
    public override bool CheckCondition()
    {
        bool keyValue = _aiController._aiBrain.blackboard.GetValue<bool>(key);
        return keyValue;
    }
}