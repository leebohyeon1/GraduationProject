using UnityEngine;
using BehaviorTree;

namespace BehaviorTree
{
    /// <summary>
    /// 블랙보드의 OnTakeHit 플래그가 설정되어 있는지 확인하는 조건 노드입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "Condition_IsHit", menuName = "BehaviorTree/Condition/IsHit")]
    public class Condition_IsHit : ConditionNode
    {
        protected override bool CheckCondition()
        {
            if (brain == null || brain.blackboard == null) return false;
            return brain.blackboard.GetValueOrDefault<bool>(EnemyBlackboardKeys.OnTakeHit, true);
        }

        public override Node Clone()
        {
            return Instantiate(this);
        }
    }
}
