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
        Debug.Log("<color=red>--STUNNED--: OnEnter</color>");
        runner.AnimationEvent(animationName);
        
    }
    protected override NodeState OnUpdate()
    {
        if (Time.time < runner.StunExitTime)
        {
            runner.Movement.StopMovement();
            return NodeState.RUNNING;
        }
        else
        {
            runner.ClearStun();
            return NodeState.SUCCESS;
        }
    }
    public override void OnExit()
    {
        // 상태를 먼저 변경하여 StopMovement의 보호 로직을 통과하게 함
        if (runner.CurrentState == Enemy.EnemyState.Rush)
        {
            runner.GetComponent<Animator>().SetBool("Rush_Running", false);
            runner.SetState(Enemy.EnemyState.Idle);
        }
    }
    public override void Abort()
    {
        base.Abort();
        runner.ClearStun();
        // 상태를 먼저 변경하여 StopMovement의 보호 로직을 통과하게 함
        if (runner.CurrentState == Enemy.EnemyState.Rush)
        {
            runner.GetComponent<Animator>().SetBool("Rush_Running", false);
            runner.SetState(Enemy.EnemyState.Idle);
        }
    }


    public override Node Clone()
    {
        return Instantiate(this);
    }
}