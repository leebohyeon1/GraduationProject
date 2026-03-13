using UnityEngine;
using BehaviorTree;

public class Condition_IsStunned : ConditionNode
{
    public StunType targetStunType = StunType.Any;

    // runner의 IsStunned() 함수를 호출하여
    // 현재 적이 기절 상태인지 확인합니다.
    protected override bool CheckCondition()
    {
        if (targetStunType == StunType.Any) return runner != null && runner.ParrySystem._isStunned;
        return runner != null && runner.ParrySystem.CurrentStun == targetStunType;
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.targetStunType = this.targetStunType;
        return node;
    }
}