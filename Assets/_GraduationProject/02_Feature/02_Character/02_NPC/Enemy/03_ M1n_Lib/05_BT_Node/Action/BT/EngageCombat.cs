// --- FILE: Action_EngageCombat.cs ---

using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "EngageCombatAction", menuName = "BehaviorTree/Action/EngageCombat")]
public class Action_EngageCombat : Node
{
    public override void OnEnter()
    {
        // runner의 전투 돌입 함수를 호출합니다.
        // 기존의 EnemyCalling()을 사용하거나, 의미에 맞게 새 함수를 만들어도 좋습니다.
        // 여기서는 기존 함수를 그대로 사용하겠습니다.
        brain.CombatEnter(); // isCalling을 true로 설정
    }

    protected override NodeState OnUpdate()
    {
        // 이 행동은 상태를 바꾸는 즉시 완료됩니다.
        return NodeState.SUCCESS;
    }
    
    public override Node Clone()
    {
        return Instantiate(this);
    }
}