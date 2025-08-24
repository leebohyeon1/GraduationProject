// --- FILE: CompositeNode.cs ---

using UnityEngine;
using BehaviorTree;

// 이 클래스는 직접 사용되지 않고 Selector, Sequence의 부모 역할을 합니다.
namespace BehaviorTree
{
    public abstract class CompositeNode : Node
    {
        public Node[] nodes;

        public override Node Clone()
        {
            CompositeNode newNode = Instantiate(this);
            newNode.nodes = new Node[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] != null) newNode.nodes[i] = nodes[i].Clone();
            }
            return newNode;
        }

        public override void initNode()
        {
            base.initNode();
            foreach (var node in nodes)
            {
                node.initNode();
            }
        }

        public override void SetRunner(Enemy runner, AiBrain brain)
        {
            base.SetRunner(runner, brain);
            foreach (var node in nodes)
            {
                node.SetRunner(runner, brain);
            }
        }
    }
}