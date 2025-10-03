using UnityEngine;
using BehaviorTree;
public class AnimationNode : Node
{
    public string animationName;


    public override Node Clone()
    {
        AnimationNode node = ScriptableObject.CreateInstance<AnimationNode>();
        node.animationName = animationName;
        return node;
    }
    public override void OnEnter()
    {
        Debug.Log("Animation Node Enter: " + animationName);
        runner.AnimationEvent(animationName);
    }

    protected override NodeState OnUpdate()
    {
        Debug.Log("Animation Node Update: " + animationName);
        Animator animator = runner.animator;
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                return NodeState.SUCCESS;
            }
        }
        return NodeState.RUNNING;
    }
}