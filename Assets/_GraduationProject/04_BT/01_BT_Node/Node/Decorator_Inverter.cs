// --- FILE: Decorator_Inverter.cs ---

using UnityEngine;
using BehaviorTree;
namespace BehaviorTree
{
    [CreateAssetMenu(fileName = "InverterDecorator", menuName = "BehaviorTree/Decorator/Inverter")]
    public class Decorator_Inverter : Node
    {
        [Tooltip("결과를 반전시킬 자식 노드")]
        public Node child;

        protected override NodeState OnUpdate()
        {
            // 먼저 자식 노드를 평가합니다.
            NodeState childState = child.Evaluate();

            // 자식의 상태에 따라 결과를 반전시킵니다.
            switch (childState)
            {
                case NodeState.SUCCESS:
                    // 자식이 성공하면, 우리는 실패합니다.
                    return NodeState.FAILURE;

                case NodeState.FAILURE:
                    // 자식이 실패하면, 우리는 성공합니다.
                    return NodeState.SUCCESS;

                case NodeState.RUNNING:
                    // 자식이 실행 중이면, 우리도 실행 중입니다.
                    return NodeState.RUNNING;
            }

            // 위에서 모든 경우가 처리되므로 이 라인은 실행되지 않지만,
            // 컴파일러를 위해 기본값을 반환합니다.
            return NodeState.FAILURE;
        }

        // --- 자식 노드를 관리하기 위한 필수 함수들 ---

        public override Node Clone()
        {
            // 자식 클래스 이름을 직접 사용 (예: Decorator_Inverter)
            var newNode = Instantiate(this);
            if (child != null)
            {
                newNode.child = child.Clone();
            }
            else
            {
                Debug.LogError($"CLONE ERROR: Decorator '{this.name}' has no child!", this);
            }
            return newNode;
        }

        public override void SetRunner(Enemy runner, AiBrain brain)
        {
            base.SetRunner(runner, brain);
            child.SetRunner(runner, brain);
        }

        public override void initNode()
        {
            base.initNode();
            child.initNode();
        }

        public override void Abort()
        {
            // 중단 신호를 자식에게 그대로 전달합니다.
            child.Abort();
            base.Abort(); // 자기 자신도 중단 처리
        }
    }
}