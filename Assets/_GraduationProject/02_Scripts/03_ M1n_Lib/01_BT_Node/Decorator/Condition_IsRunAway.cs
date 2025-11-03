using UnityEngine;
using BehaviorTree;
public class Condition_IsRunAway : ConditionNode
{
    [Tooltip("이 거리보다 멀어야 합니다.")]
    public float targetDistance;

    protected override bool CheckCondition()
    {
        if (runner == null || runner.player == null)
        {
            return false;
        }
        if (runner.CurrentState == Enemy.EnemyState.Idle)
            return true;
        return false;
    }

    public override Node Clone()
    {
        // 인스펙터에서 설정한 값들을 복제본에 그대로 복사합니다.
        Condition_IsRunAway node = Instantiate(this);
        node.targetDistance = this.targetDistance;
        return node;
    }
}