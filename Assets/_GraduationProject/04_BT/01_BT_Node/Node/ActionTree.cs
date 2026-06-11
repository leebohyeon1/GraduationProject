using UnityEngine;
using BehaviorTree; // 우리 네임스페이스

namespace BehaviorTree
{
    // ActionTree는 Behavior Tree의 액션 노드를 관리하는 ScriptableObject입니다.
    // 이 트리는 ActionNode와 같은 액션 노드들을 포함할 수 있습니다.
    // ActionTree는 Behavior Tree의 시작점이 될 수 있습니다.
    [CreateAssetMenu(fileName = "ActionTree", menuName = "BehaviorTree/Action Tree")]
    public class ActionTree : ScriptableObject
    {
        // 이 트리의 시작점이 되는 루트 노드입니다.
        public Node rootNode;
        public ActionTree Clone()
        {
            // 1. ActionTree 자신의 얕은 복사본을 만듭니다.
            ActionTree newTree = Instantiate(this);

            // 2. 루트 노드부터 시작하여 모든 노드를 깊은 복사(Deep Copy)합니다.
            //    이렇게 하면 모든 노드가 새로 생성되어 상태를 공유하지 않습니다.
            if (rootNode != null)
            {
                newTree.rootNode = rootNode.Clone();
            }

            return newTree;
        }

        // (추가) 복제된 트리에 실행자(Enemy)를 설정하는 유틸리티 함수
        public void SetRunner(Enemy runner, AiBrain brain)
        {
            if (rootNode != null)
            {
                rootNode.SetRunner(runner, brain);
            }
        }
    }
    
}