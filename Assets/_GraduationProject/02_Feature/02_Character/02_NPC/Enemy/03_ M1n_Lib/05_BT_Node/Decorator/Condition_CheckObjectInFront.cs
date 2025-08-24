using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "CheckObjectInFront_Condition", menuName = "BehaviorTree/Condition/CheckObjectInFront")]
public class Condition_CheckObjectInFront : ConditionNode
{
    [Tooltip("감지할 오브젝트의 레이어를 설정합니다.")]
    public LayerMask targetLayer;

    [Tooltip("감지할 거리입니다.")]
    public float detectionDistance = 1.5f;

    protected override bool CheckCondition()
    {
        // 1. 마지막으로 충돌한 오브젝트가 없으면 무조건 실패
        if (runner.GetLastRushHitObject() == null)
        {
            return false;
        }

        // 2. 마지막으로 충돌한 오브젝트의 레이어가 우리가 찾는 레이어와 일치하는지 확인
        Debug.Log($"--CHECK FRONT--: Last hit object layer is {LayerMask.LayerToName(runner.GetLastRushHitObject().layer)}.");
        return (targetLayer.value & (1 << runner.GetLastRushHitObject().layer)) > 0;
    }

    public override Node Clone()
    {
        var node = Instantiate(this);
        node.targetLayer = this.targetLayer;
        return node;
    }
}