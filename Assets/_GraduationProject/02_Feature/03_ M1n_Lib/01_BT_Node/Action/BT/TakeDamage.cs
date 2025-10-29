using BehaviorTree;
using UnityEngine;

public class TakeDamage : Node
{
    public string animationName;
    public override void OnEnter()
    {
        Debug.Log($"<color=red>{runner.name} TakeDamage Node OnEnter</color>");
        Handler.ResetAllFlags();
        if (runner.CurrentState != Enemy.EnemyState.Die)
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
        if (Handler.IsActionFinished)
            return NodeState.SUCCESS;
        return NodeState.RUNNING;
    }
    public override void OnExit()
    {
        runner.SetState(Enemy.EnemyState.Idle);
    }
    public override void Abort()
    {
        runner.SetState(Enemy.EnemyState.Idle);
        
    }
}