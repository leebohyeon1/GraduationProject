using UnityEngine;
using BehaviorTree;
public class AnimationNode : Node
{
    public string animationName;
    public enum AnimationType
    {
        Trigger,
        True,
        False
    }

    public AnimationType animationBool = AnimationType.Trigger;
    public EnemyUseAnything enemyUseAnything;
    public override Node Clone()
    {
        AnimationNode node = ScriptableObject.CreateInstance<AnimationNode>();
        node.animationName = animationName;
        node.animationBool = animationBool;
        node.enemyUseAnything = enemyUseAnything;
        return node;
    }
    public override void OnEnter()
    {
        enemyUseAnything?.OnEnter(runner);

        runner.Movement.StopMovement();
        if(animationBool != AnimationType.Trigger)
        {
            runner.animator.SetBool(animationName, animationBool == AnimationType.True);
        }
        else
        {
            runner.AnimationEvent(animationName);
        }
    }

    protected override NodeState OnUpdate()
    {
        Animator animator = runner.animator;
        if (Handler.IsActive)
        {
            enemyUseAnything?.OnUpdate(runner);
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName(animationName))
        {
            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.95f)
            {
                return NodeState.SUCCESS;
            }
        }
        // Debug.Log($"AnimationNode {animationName} running");
        return NodeState.RUNNING;
    }
}