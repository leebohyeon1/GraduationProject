using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "Task_HitAction", menuName = "BehaviorTree/Action/HitAction")]
public class Task_HitAction : Node
{
    private int _entryFrame;

    public override void OnEnter()
    {
        base.OnEnter();
        if (Handler != null) Handler.ResetAllFlags();
        
        runner.SetState(EnemyStateController.EnemyState.Hit);
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
        runner.Movement.StopMovement();

        return NodeState.RUNNING;
    }

    public override void OnExit()
    {
        base.OnExit();
        
        brain.blackboard.SetValue(EnemyBlackboardKeys.OnTakeHit, false);
        
        if (runner.CurrentState == EnemyStateController.EnemyState.Hit)
        {
            runner.SetState(EnemyStateController.EnemyState.Idle);
        }

        if (Handler != null) Handler.ResetAllFlags();
        runner._stateController.SetLock(false);

        
    }

    public override Node Clone()
    {
        return Instantiate(this);
    }
}
