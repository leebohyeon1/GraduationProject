using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "IsBetweenDistances_Condition", menuName = "BehaviorTree/Condition/IsBetweenDistances")]
public class Condition_IsBetweenDistances : ConditionNode
{
    [Tooltip("이 거리보다는 멀어야 합니다 (최소 거리).")]
    public float minDistance;

    [Tooltip("이 거리보다는 가까워야 합니다 (최대 거리).")]
    public float maxDistance;

    protected override bool CheckCondition()
    {
        if (runner == null || runner.player == null)
        {
            return false;
        }

        // 플레이어와의 실제 거리를 계산합니다.
        float distanceToPlayer = Vector3.Distance(runner.transform.position, runner.player.transform.position);

        // 거리가 최소 거리와 최대 거리 사이에 있는지 확인합니다.
        bool isInRange = distanceToPlayer >= minDistance && distanceToPlayer <= maxDistance;
        
        return isInRange;
    }

    public override Node Clone()
    {
        // 인스펙터에서 설정한 값들을 복제본에 그대로 복사합니다.
        Condition_IsBetweenDistances node = Instantiate(this);
        node.minDistance = this.minDistance;
        node.maxDistance = this.maxDistance;
        return node;
    }
}