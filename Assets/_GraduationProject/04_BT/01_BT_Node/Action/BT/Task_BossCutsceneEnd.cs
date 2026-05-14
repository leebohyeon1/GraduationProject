using BehaviorTree;

public class Task_BossCutsceneEnd : Node
{

    public override void OnEnter()
    {
        base.OnEnter();
    }
    protected override NodeState OnUpdate()
    {
        return NodeState.FAILURE;
    }

    public override void OnExit()
    {

        runner.enemyStat.EStateEventSO?.Publish(new EnemyStateData{
            enemy = runner, stateType = EnemyStateType.SummonBoss});
    }
    
    public override Node Clone()
    {
        Task_BossCutsceneEnd node = Instantiate(this);
        return node;
    }
}