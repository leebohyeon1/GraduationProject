using BehaviorTree;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace BehaviorTree
{
    
    public abstract class ServiceNode : Node
    {
        public float UpdateInterval = 0.1f;
        protected float lastExecutionTime = 0f;

        public override Node Clone()
        {
            ServiceNode node = ScriptableObject.CreateInstance<ServiceNode>();
            node.UpdateInterval = this.UpdateInterval;
            return node;
        }

        protected override NodeState OnUpdate()
        {
            if(Time.time - lastExecutionTime >= UpdateInterval)
            {
                OnServiceLogic();
                lastExecutionTime = Time.time;
            }
            return NodeState.SUCCESS;

        }
        protected abstract void OnServiceLogic();
        public override void initNode()
        {
            base.initNode();
            lastExecutionTime = -UpdateInterval; 
        }
    }
}
