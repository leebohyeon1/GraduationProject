using BehaviorTree;
using UnityEngine;

public class TakeDamage : Node
{
    public string animationName;
    public override void OnEnter()
    {
        runner.ResetActionFlags();
        if(runner.CurrentState != Enemy.EnemyState.Die)
        runner.AnimationEvent(animationName);
    }
    public override Node Clone()
    {
        TakeDamage runner = Instantiate(this);
        runner.animationName = this.animationName;
        return runner;
    }

    protected override NodeState OnUpdate()
    {
        Debug.Log("TakeDamage 노드 업데이트");
        if (runner.IsActionFinished)
            return NodeState.SUCCESS;
        return NodeState.RUNNING;
    }
    public override void OnExit()
    {
        Debug.Log("TakeDamage 노드 종료");
        runner.SetState(Enemy.EnemyState.Idle);
    }
}