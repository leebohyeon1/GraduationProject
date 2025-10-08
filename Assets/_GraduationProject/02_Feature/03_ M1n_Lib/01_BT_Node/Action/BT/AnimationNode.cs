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
        runner.Movement.StopMovement();
        runner.AnimationEvent(animationName);
    }

    protected override NodeState OnUpdate()
    {
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