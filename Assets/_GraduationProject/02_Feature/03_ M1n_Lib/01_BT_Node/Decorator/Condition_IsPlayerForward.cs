using UnityEngine;
using BehaviorTree;

public class Condition_IsPlayerBehind : ConditionNode
{
    public float angleThreshold = 45f; // 플레이어가 뒤에 있다고 간주하는 각도 범위

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.angleThreshold = this.angleThreshold;
        return node;
    }

    protected override bool CheckCondition()
    {
        Vector3 toPlayer = runner.transform.position - runner.player.transform.position;
        toPlayer.y = 0; // 수평면에서의 벡터만 고려
        Vector3 forward = runner.player.transform.forward;
        forward.y = 0; // 수평면에서의 벡터만 고려

        float angleToPlayer = Vector3.Angle(forward, toPlayer);

        // 플레이어의 뒤쪽에 있는지 확인
        return angleToPlayer > (180f - angleThreshold / 2f);
    }
}