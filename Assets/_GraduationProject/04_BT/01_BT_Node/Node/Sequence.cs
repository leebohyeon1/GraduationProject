using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using UnityEngine.PlayerLoop;
namespace BehaviorTree
{
    [CreateAssetMenu(fileName = "SequenceNode", menuName = "BehaviorTree/Sequence")]
    public class Sequence : CompositeNode
    {
        // [System.NonSerialized]
        private int _currentChildIndex;

        // Sequence 노드에 처음 진입할 때, 인덱스를 0으로 초기화합니다.
        public override void OnEnter()
        {
            base.OnEnter(); // 디버깅을 위해 부모의 OnEnter도 호출
            _currentChildIndex = 0;
        }

        protected override NodeState OnUpdate()
        {
            UpdateServices();
            // 이전에 실행 중이던 자식부터 이어서 검사합니다.
            for (int i = _currentChildIndex; i < nodes.Length; i++)
            {
                // 현재 실행할 자식의 인덱스를 저장합니다.
                _currentChildIndex = i;
                Node child = nodes[i];
                NodeState state = child.Evaluate();

                // 자식이 실패하면, Sequence 전체가 즉시 실패입니다.
                if (state == NodeState.FAILURE)
                {
                    // 실패했으므로, 더 이상 실행 중인 자식은 없습니다.
                    // 다음 실행을 위해 인덱스를 리셋할 수도 있지만, OnEnter에서 처리하므로 여기서 꼭 필요하진 않습니다.
                    return NodeState.FAILURE;
                }

                // 자식이 아직 실행 중이면, Sequence도 실행 중입니다.
                // 다음 프레임에 이어서 실행하기 위해 상태를 그대로 반환합니다.
                if (state == NodeState.RUNNING)
                {
                    return NodeState.RUNNING;
                }

                // 자식이 성공(SUCCESS)했다면, for 루프는 다음 자식으로 계속 진행됩니다.
            }

            // for 루프가 모두 끝났다는 것은 모든 자식들이 성공했다는 의미입니다.
            return NodeState.SUCCESS;
        }

        public override void initNode()
        {
            base.initNode();
            _currentChildIndex = 0;
        }

        // Sequence 자신이 중단될 때, 실행 중이던 자식도 확실히 정리합니다.
        public override void Abort()
        {
            // 자기 자신이 실행 중이었을 때만 처리합니다.
            if (isEntered)
            {
                // Sequence 내부에서 실행 중이던 자식이 있었다면
                if (_currentChildIndex < nodes.Length && nodes[_currentChildIndex] != null)
                {
                    // 그 자식에게 중단 신호를 전달합니다.
                    nodes[_currentChildIndex].Abort();
                }
                // base.Abort()는 Node.cs의 Abort를 호출하여 isEntered를 false로 만듭니다.
                base.Abort();
            }
        }
    }
}
