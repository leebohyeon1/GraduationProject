using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "Stunned", menuName = "BehaviorTree/Stunned")]
public class Stunned : Node
{
    public override void OnEnter()
    {
        base.OnEnter();
        runner.SetState(EnemyStateController.EnemyState.Stunned);
        if(runner.Shield != null)
        runner.Shield.IsActive = false;
        Debug.Log("<color=red>--STUNNED--: OnEnter Triggered</color>");
    }
    protected override NodeState OnUpdate()
    {
        if (runner.ParrySystem._isStunned && runner.ParrySystem.StunExitTime <= Time.time)
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

    }
    public override void OnExit()
    {
        runner.ParrySystem.ClearStun();
        if(runner.Shield != null)
        runner.Shield.IsActive = true;
        Debug.Log("<color=red>--STUNNED--: OnExit Triggered</color>");
        runner.SetState(EnemyStateController.EnemyState.Idle);
        Handler.ResetAllFlags();
    }



    public override Node Clone()
    {
        return Instantiate(this);
    }
}