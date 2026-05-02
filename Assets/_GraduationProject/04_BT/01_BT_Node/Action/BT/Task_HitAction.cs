using UnityEngine;
using BehaviorTree;
using System.Diagnostics.Contracts;
using System;

[CreateAssetMenu(fileName = "Task_HitAction", menuName = "BehaviorTree/Action/HitAction")]
public class Task_HitAction : Node
{
    private int _entryFrame;
    private float _hitStartTime;

    public override void OnEnter()
    {
        base.OnEnter();
        if (Handler != null) Handler.ResetAllFlags();
        runner._animationBridge.ResetAllAnimationStates();

        runner.AnimationEvent("Hit");
        runner.SetState(EnemyStateController.EnemyState.Hit);
        _hitStartTime = brain.blackboard.GetValue<float>("OnTaskHit");
        runner._stateController.SetLock(true);
        runner.animator.speed = 1f;
        runner.animHandler.SpeedMultiplier = 1f;
        if (runner.Movement != null) runner.Movement.StopMovement();
        Debug.Log($"[Task_HitAction] OnEnter called, OnTakeHit: {_hitStartTime}, OnTaskHit: {brain.blackboard.GetValue<float>("OnTaskHit")}");
    }

    protected override NodeState OnUpdate()
    {
        if (runner == null) return NodeState.FAILURE;
        runner._animationBridge.ResetAllAnimationStates("Hit");
        
        if(_hitStartTime != brain.blackboard.GetValue<float>("OnTaskHit"))
        {
            Debug.Log($"[Task_HitAction] OnUpdate called, OnTakeHit: {_hitStartTime}, OnTaskHit: {brain.blackboard.GetValue<float>("OnTaskHit")}");
            runner.AnimationEvent("Hit");
            _hitStartTime = brain.blackboard.GetValue<float>("OnTaskHit");
        }
        if (Time.frameCount <= _entryFrame + 1) return NodeState.RUNNING;

        if (Handler != null && Handler.IsActionFinished)
        {
            return NodeState.SUCCESS;
        }
        runner.Movement.StopMovement();
        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log($"[Task_HitAction] OnExit called, resetting hit flags.");
        brain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, false);
        brain.blackboard.SetValue("OnTaskHit", 0f);
        
        runner._stateController.SetLock(false);
        if (runner.CurrentState == EnemyStateController.EnemyState.Hit)
        {
            runner.SetState(EnemyStateController.EnemyState.Idle);
        }

        if (Handler != null) Handler.ResetAllFlags();
    }
    public override void Abort()
    {
        base.Abort();
        OnExit();
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}
