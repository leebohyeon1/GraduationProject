using UnityEngine;
using BehaviorTree;

public class Condition_IsPlayerBehind : ConditionNode
{
    public float playerViewAngle = 90f;
    public override Node Clone()
    {
        var node = Instantiate(this);
        node.playerViewAngle = this.playerViewAngle;
        return node;
    }

    protected override bool CheckCondition()
    {
        Vector3 playerForward = runner.player.transform.forward;
        playerForward.y = 0; // 수평면에서만 계산
        playerForward.Normalize();

        // 2. 플레이어로부터 Enemy(자신)를 향하는 방향 벡터
        Vector3 directionToEnemy = runner.transform.position - runner.player.transform.position;
        directionToEnemy.y = 0; // 수평면에서만 계산
        directionToEnemy.Normalize();

        // 3. 두 벡터 사이의 각도를 계산
        float angle = Vector3.Angle(playerForward, directionToEnemy);

        // 4. 계산된 각도가 설정된 시야각보다 작으면, 플레이어가 Enemy를 보고 있는 것으로 판단
        return angle < playerViewAngle / 2f;
    }
}