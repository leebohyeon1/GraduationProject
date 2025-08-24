// --- FILE: Selector.cs (Clean Version) ---
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
            for (int i = 0; i < nodes.Length; i++)
            {
                NodeState state = nodes[i].Evaluate();
                if (state != NodeState.FAILURE)
                {
                    if (_runningChildIndex != -1 && _runningChildIndex != i) { nodes[_runningChildIndex].Abort(); }
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