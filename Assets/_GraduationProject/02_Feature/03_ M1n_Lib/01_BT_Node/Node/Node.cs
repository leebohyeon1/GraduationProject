// --- FILE: Node.cs ---

using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace BehaviorTree
{
    public abstract class Node : ScriptableObject
    {
        public Vector2 position;
        public enum NodeState { SUCCESS, FAILURE, RUNNING }

        [System.NonSerialized] protected bool isEntered = false;
        protected Enemy runner;
        protected AiBrain brain;
        protected Enemy_AnimationEventHandler Handler => runner.animHandler;
        public NodeState Evaluate()
        {
            if (!isEntered)
            {
                OnEnter();
                isEntered = true;
            }
            NodeState currentState = OnUpdate();
            if (currentState != NodeState.RUNNING)
            {
                OnExit();
                isEntered = false;
            }

            // if(currentState == NodeState.SUCCESS)
            // Debug.Log($"runner: {runner.name}, Node: {this.name}, State: <color=green>{currentState}</color>");

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

        public virtual void OnEnter()
        {
            
        }

        public virtual void OnExit() { }
        protected abstract NodeState OnUpdate();

        public abstract Node Clone();
        public virtual void initNode() { isEntered = false; }
        public virtual void SetRunner(Enemy runner, AiBrain brain) { this.runner = runner; this.brain = brain; }
    }
}