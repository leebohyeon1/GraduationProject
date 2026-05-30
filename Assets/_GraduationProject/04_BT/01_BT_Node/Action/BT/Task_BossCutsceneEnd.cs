using BehaviorTree;

public class Task_BossCutsceneEnd : Node
{

    public override void OnEnter()
    {
        base.OnEnter();
    }
    protected override NodeState OnUpdate()
    {
        return NodeState.SUCCESS;
    }

    public override void OnExit()
    {
        runner.StateType = EnemyStateType.SummonBoss;
    }
    
    public override Node Clone()
    {
        Task_BossCutsceneEnd node = Instantiate(this);
        return node;
    }
}