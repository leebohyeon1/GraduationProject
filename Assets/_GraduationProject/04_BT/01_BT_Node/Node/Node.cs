// --- FILE: Node.cs ---

using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace BehaviorTree
{
    public abstract class Node : ScriptableObject
    {
        private const bool ENABLE_BT_TRACE_LOG = false;
        public Vector2 position;
        public enum NodeState { SUCCESS, FAILURE, RUNNING }

        [System.NonSerialized] protected bool isEntered = false;
        protected Enemy runner;
        protected AiBrain brain;
        protected Enemy_AnimationEventHandler Handler => runner.animHandler;

        public static object CurrentNodeName { get; protected set; }

        public NodeState Evaluate()
        {
            if (!isEntered)
            {
                if (ENABLE_BT_TRACE_LOG) BTDebug.Log(string.Format("[BT] Node Enter: {0} ({1})", this.name, this.GetType().Name));
                CurrentNodeName = this.name;
                OnEnter();
                isEntered = true;
            }
            NodeState currentState = OnUpdate();

            if (currentState != NodeState.RUNNING)
            {
                OnExit();
                if (ENABLE_BT_TRACE_LOG) BTDebug.Log(string.Format("[BT] Node Exit: {0} ({1}) -> {2}", this.name, this.GetType().Name, currentState));
                isEntered = false;
            }
            return currentState;
        }

        public virtual void Abort()
        {
            if (isEntered)
            {
                OnExit();
                isEntered = false;
            }
        }

        public virtual void OnEnter(){}

        public virtual void OnExit(){}
        protected abstract NodeState OnUpdate();

        public virtual Node Clone()
        {
            return Instantiate(this);
            
        }
        
        public virtual void initNode() { isEntered = false; }
        public virtual void SetRunner(Enemy runner, AiBrain brain) { this.runner = runner; this.brain = brain; }
    }
}
