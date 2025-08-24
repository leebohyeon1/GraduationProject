using BehaviorTree;
using UnityEngine;

[CreateAssetMenu(fileName = "IsActionable_Condition", menuName = "BehaviorTree/Condition/IsActionable")]
public class Condition_IsActionable : ConditionNode
{
    protected override bool CheckCondition()
    {
        // runner(Enemy)가 방해할 수 없는 상태가 "아닐 때" true를 반환합니다.
        return runner != null && !brain.IsActionable();
    }

    public override Node Clone() => Instantiate(this);
}