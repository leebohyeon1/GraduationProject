using UnityEngine;
using BehaviorTree;

public class Condition_CanSeePlayer : ConditionNode
{
    public float viewAngle = 90f;
    protected override bool CheckCondition()
    {
        if (runner == null || runner.player == null)
        {
            return false;
        }

        Vector3 toPlayer = runner.player.transform.position - runner.transform.position;

        // 2. 적의 시야각(예: 90도) 안에 플레이어가 있는지 확인
        // runner.DetectionAngle 같은 변수가 Enemy.cs에 추가되어야 합니다.
        if (Vector3.Angle(runner.transform.forward, toPlayer.normalized) > viewAngle * 0.5f)
        {
            return false;
        }

        // 모든 조건을 통과했으면, 플레이어를 볼 수 있다는 의미이므로 true를 반환합니다.
        // (추가: Raycast를 통해 벽과 같은 장애물이 있는지 확인하는 로직을 넣으면 더욱 완벽해집니다.)
        return true;
    }
        public override Node Clone() => Instantiate(this);

}