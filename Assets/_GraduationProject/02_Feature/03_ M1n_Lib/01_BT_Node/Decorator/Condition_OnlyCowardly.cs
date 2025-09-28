using BehaviorTree;
using UnityEngine;
[CreateAssetMenu(fileName = "Condition_OnlyCowardly", menuName = "BehaviorTree/Condition/OnlyCowardly")]
public class Condition_OnlyCowardly : ConditionNode
{
    public override Node Clone()
    {
        var node = Instantiate(this);
        return node;
    }

    protected override bool CheckCondition()
    {
        if (runner.groupAi.OnlyCowardly())
        {
            return true;
        }
        return false;
    }
}