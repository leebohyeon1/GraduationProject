using UnityEngine;
using BehaviorTree;



namespace BehaviorTree
{
    public abstract class ConditionNode : Node
    {

        protected override NodeState OnUpdate()
        {
            // 자식 클래스에서 구현한 조건을 확인하고,
            // 그 결과에 따라 SUCCESS 또는 FAILURE를 즉시 반환합니다.
            return CheckCondition() ? NodeState.SUCCESS : NodeState.FAILURE;
        }

        // 이 클래스를 상속받는 클래스는 이 함수를 구현하여 실제 조건을 검사해야 합니다.
        protected abstract bool CheckCondition();

        public override void Abort()
        {
            base.Abort();
        }
    }
}