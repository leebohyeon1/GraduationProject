using UnityEngine;
using BehaviorTree;
namespace BehaviorTree
{
    [CreateAssetMenu(fileName = "SelectorNode", menuName = "BehaviorTree/Selector")]
    public class Selector : CompositeNode
    {
        [System.NonSerialized]
        private int _runningChildIndex = -1;

        protected override NodeState OnUpdate()
        {
            UpdateServices();

            // [핵심 수정] 상태가 잠겨(Locked) 있다면 우선순위 재평가를 중단합니다.
            // 공격 시퀀스 등이 진행 중일 때 더 높은 우선순위의 노드(예: 도망)가 이를 가로채지 못하도록 방어합니다.
            if (runner._stateController != null && runner._stateController.IsStateLocked && _runningChildIndex != -1)
            {
                NodeState state = nodes[_runningChildIndex].Evaluate();
                if (state != NodeState.FAILURE)
                {
                    _runningChildIndex = (state == NodeState.RUNNING) ? _runningChildIndex : -1;
                    return state;
                }
                // 현재 노드가 실패했다면 아래 루프를 통해 다시 검색을 허용합니다.
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                NodeState state = nodes[i].Evaluate();
                if (state != NodeState.FAILURE)
                {
                    if (_runningChildIndex != -1 && _runningChildIndex != i) 
                    { 
                        nodes[_runningChildIndex].Abort(); 
                    }
                    _runningChildIndex = (state == NodeState.RUNNING) ? i : -1;
                    return state;
                }
            }
            _runningChildIndex = -1;
            return NodeState.FAILURE;
        }

        public override void initNode()
        {
            base.initNode();
            _runningChildIndex = -1;
        }

        public override void Abort()
        {
            if (isEntered)
            {
                if (_runningChildIndex != -1)
                {
                    nodes[_runningChildIndex].Abort();
                    _runningChildIndex = -1;
                }
                base.Abort();
            }
        }
    }
}
