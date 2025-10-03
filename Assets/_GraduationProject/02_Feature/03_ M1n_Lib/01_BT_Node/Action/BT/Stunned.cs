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
        runner.SetState(Enemy.EnemyState.Stunned);
        
    }
    protected override NodeState OnUpdate()
    {
        runner.Movement.StopMovement();
        if (Time.time >= runner.ParrySystem.StunExitTime && runner.ParrySystem._isStunned)
        {
            runner.ParrySystem.ClearStun();
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
        // 상태를 먼저 변경하여 StopMovement의 보호 로직을 통과하게 함
        if (runner.CurrentState == Enemy.EnemyState.Rush)
        {
            runner.GetComponent<Animator>().SetBool("Rush_Running", false);
        }
        runner.SetState(Enemy.EnemyState.Idle);
    }
    public override void Abort()
    {
        base.Abort();
        // 상태를 먼저 변경하여 StopMovement의 보호 로직을 통과하게 함
        if (runner.CurrentState == Enemy.EnemyState.Rush)
        {
            runner.GetComponent<Animator>().SetBool("Rush_Running", false);
        }
        runner.SetState(Enemy.EnemyState.Idle);
    }


    public override Node Clone()
    {
        return Instantiate(this);
    }
}