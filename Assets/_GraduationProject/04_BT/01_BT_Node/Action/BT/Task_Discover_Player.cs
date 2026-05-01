using UnityEngine;
using BehaviorTree;

public class Task_Discover_Player : Node
{
    private float _entryTime;
    public float transitionBuffer = 0.5f;
    public string animationTagName = "Discover_Player";
    private bool _didSetLock = false;

    public override void OnEnter()
    {
        _entryTime = Time.time;
        _didSetLock = false;
        
        if (!brain._isCombat)
        {
            // runner._aiController._aiBrain.blackboard.SetValue("Engage", true);
            runner.AnimationEvent(animationTagName);
            if (Handler != null) Handler.ResetAllFlags();
            runner._stateController.SetState(EnemyStateController.EnemyState.Discover);
            runner._stateController.SetLock(true);
            _didSetLock = true;
        }
    }

    protected override NodeState OnUpdate()
    {
        if (runner.CurrentState == EnemyStateController.EnemyState.Stunned || runner.CurrentState == EnemyStateController.EnemyState.Die)
        {
            return NodeState.FAILURE;
        }

        float elapsedTime = Time.time - _entryTime;

        var stateInfo = runner.animator.GetCurrentAnimatorStateInfo(0);
        var nextStateInfo = runner.animator.GetNextAnimatorStateInfo(0);
        bool isTagActive = stateInfo.IsTag(animationTagName) || nextStateInfo.IsTag(animationTagName);

        if (Handler != null && Handler.IsActionFinished)
        {
            return NodeState.SUCCESS;
        }
        if(brain.blackboard.GetValue<bool>(EnemyBlackboardKeys.OnTakeHit))
        {
            return NodeState.FAILURE;
        }
        if (isTagActive || elapsedTime < transitionBuffer)
        {
            return NodeState.RUNNING;
        }

        if (elapsedTime > transitionBuffer + 2.0f)
        {
             return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        if (_didSetLock && runner._stateController != null)
        {
            runner._stateController.SetLock(false);
            _didSetLock = false;
            // brain.CombatEnter(true);
            // if (runner.groupAi != null) runner.groupAi.CombatAll();
        }
        if (Handler != null) Handler.ResetAllFlags();
    }

    public override void Abort()
    {
        if (_didSetLock && runner._stateController != null)
        {
            runner._stateController.SetLock(false);
            _didSetLock = false;
        }
        if (Handler != null) Handler.ResetAllFlags();
        base.Abort();
    }

    public override Node Clone()
    {
        Task_Discover_Player node = Instantiate(this);
        node.transitionBuffer = this.transitionBuffer;
        node.animationTagName = this.animationTagName;
        return node;
    }
}
