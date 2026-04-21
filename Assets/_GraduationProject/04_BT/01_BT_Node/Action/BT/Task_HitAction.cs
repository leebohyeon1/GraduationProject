using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "Task_HitAction", menuName = "BehaviorTree/Action/HitAction")]
public class Task_HitAction : Node
{
    private int _entryFrame;
    private float _entryTime;
    [Tooltip("Hit 종료 이벤트가 누락되었을 때 강제 종료할 최대 시간(초)")]
    public float fallbackDuration = 0.9f;

    public override void OnEnter()
    {
        base.OnEnter();
        _entryFrame = Time.frameCount;
        _entryTime = Time.time;
        if (Handler != null) Handler.ResetAllFlags();
        
        runner.SetState(EnemyStateController.EnemyState.Hit);
        runner.AnimationEvent("Hit");
        runner._stateController.SetLock(true);
        if (runner.Movement != null) runner.Movement.StopMovement();
        
    }

    protected override NodeState OnUpdate()
    {
        if (runner == null) return NodeState.FAILURE;

        if (Time.frameCount <= _entryFrame + 1) return NodeState.RUNNING;

        if (Handler != null && Handler.IsActionFinished)
        {
            return NodeState.SUCCESS;
        }

        if (Time.time - _entryTime >= fallbackDuration)
        {
            return NodeState.SUCCESS;
        }
        runner.Movement.StopMovement();
        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        base.OnExit();
        brain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, false);
        
        runner._stateController.SetLock(false);
        runner.SetState(EnemyStateController.EnemyState.Idle);
        

        if (Handler != null) Handler.ResetAllFlags();

        
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}
