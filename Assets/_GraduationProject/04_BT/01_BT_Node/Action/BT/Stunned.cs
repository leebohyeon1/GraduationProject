using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "Stunned", menuName = "BehaviorTree/Stunned")]
public class Stunned : Node
{
    public override void OnEnter()
    {
        base.OnEnter();
        runner.SetState(EnemyStateController.EnemyState.Stunned);
        Debug.Log("<color=red>--STUNNED--: OnEnter Triggered</color>");
    }
    protected override NodeState OnUpdate()
    {
        if (Handler.IsActionFinished && runner.ParrySystem._isStunned)
        {
            Debug.Log("<color=red>--STUNNED--: OnUpdate Finished</color>");
            return NodeState.SUCCESS;
        }
        if(!runner.ParrySystem._isStunned)
        {
            return NodeState.FAILURE;
        }
        else
        {
        runner.Movement.StopMovement();

            return NodeState.RUNNING;
        }
        // if (!Handler.IsActionFinished)
        // {
        //     return NodeState.RUNNING;
        // }
        // else
        // {
        //     runner.ClearStun();
        //     Debug.Log("<color=red>--STUNNED--: OnUpdate Finished</color>");
        //     return NodeState.SUCCESS;
        // }
    }
    public override void OnExit()
    {
        runner.ParrySystem.ClearStun();
        Debug.Log("<color=red>--STUNNED--: OnExit Triggered</color>");
        runner.SetState(EnemyStateController.EnemyState.Idle);
        Handler.ResetAllFlags();
    }
    public override void Abort()
    {
        base.Abort();
        runner.SetState(EnemyStateController.EnemyState.Idle);
        Handler.ResetAllFlags();
    }


    public override Node Clone()
    {
        return Instantiate(this);
    }
}