using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "IsInCombatCondition", menuName = "BehaviorTree/Condition/IsInCombat")]
public class Condition_IsInCombat : ConditionNode
{
    // runner의 IsCalling() (또는 IsInCombat()) 함수를 호출하여
    // 현재 전투 상태인지 확인합니다.
    protected override bool CheckCondition()
    {
        return runner != null && brain._isCombat; // isCalling이 true일 때만 성공
    }
    public override Node Clone() => Instantiate(this);

}