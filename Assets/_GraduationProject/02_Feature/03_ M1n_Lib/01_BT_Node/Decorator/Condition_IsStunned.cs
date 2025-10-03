using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "IsStunnedCondition", menuName = "BehaviorTree/Condition/IsStunned")]
public class Condition_IsStunned : ConditionNode
{
    // runner의 IsStunned() 함수를 호출하여
    // 현재 적이 기절 상태인지 확인합니다.
    protected override bool CheckCondition()
    {
        return runner != null && runner.ParrySystem._isStunned; // isStunned가 true일 때만 성공
    }

    public override Node Clone() => Instantiate(this);
}