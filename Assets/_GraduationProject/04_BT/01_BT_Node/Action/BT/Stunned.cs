using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "Stunned", menuName = "BehaviorTree/Stunned")]
public class Stunned : Node
{
    public string animationName = "Do_Stun";
    public override void OnEnter()
    {
        base.OnEnter();
        // runner.parryied();
        runner.AnimationEvent(animationName);
        runner.Movement.StopMovement();
        runner.SetState(Enemy.EnemyState.Stunned);
        Debug.Log("<color=red>--STUNNED--: OnEnter Triggered</color>");
    }
    protected override NodeState OnUpdate()
    {
        if (Handler.IsActionFinished && runner.ParrySystem._isStunned)
        {
            Debug.Log("<color=red>--STUNNED--: OnUpdate Finished</color>");
            return NodeState.SUCCESS;
        }
        else
        {
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

        runner.SetState(Enemy.EnemyState.Idle);
        Handler.ResetAllFlags();
    }
    public override void Abort()
    {
        base.Abort();
        runner.SetState(Enemy.EnemyState.Idle);
        Handler.ResetAllFlags();
    }


    public override Node Clone()
    {
        return Instantiate(this);
    }
}