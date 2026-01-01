using UnityEngine;
using BehaviorTree;

public class Decorator_CheckLeash : ConditionNode
{
    [Header("Settings")]
    public float leashDistance = 20.0f; // 이 거리보다 멀어지면 복귀 시작

    protected override bool CheckCondition()
    {
        // 1. 블랙보드에서 데이터 가져오기
        // (블랙보드 시스템 구현 방식에 따라 접근 코드는 다를 수 있습니다)
        Vector3 homePos = runner._aiController._aiBrain.blackboard.GetValue<Vector3>("HomePosition");


        // 2. 거리 계산 (평면 거리 계산 권장 - Y축 무시)
        float dist = Vector3.Distance(new Vector3(runner.transform.position.x, 0, runner.transform.position.z), 
                                      new Vector3(homePos.x, 0, homePos.z));

        // 3. 판단
        return dist > leashDistance;
    }

    public override Node Clone()
    {
       return Instantiate(this);
    }
}