using UnityEngine;
using BehaviorTree;
public class AnimationNode : Node
{
    public string animationName;
    public bool OnSuccess = false;
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
        node.OnSuccess = OnSuccess;
        return node;
    }
    public override void OnEnter()
    {
        enemyUseAnything?.OnEnter(runner);

        runner.Movement.StopMovement();
        Debug.Log("iscombat " + runner._aiController._aiBrain._isCombat);
        Debug.Log($"AnimationNode {animationName} entered");
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
        if(OnSuccess)
        {
            return NodeState.SUCCESS;
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