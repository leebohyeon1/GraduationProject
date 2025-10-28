using UnityEngine;
using BehaviorTree;

public class OverHeatUse : Node
{
    public EnemyUseAnything overHeatUseSO;
    public override Node Clone()
    {
        return Instantiate(this);
    }
    public override void OnEnter()
    {
        runner.heatSystem.OverHeatUse();
        overHeatUseSO.OnEnter(runner);
        runner.EnemyHealth.SetKnockbackable(true);
    }
    protected override NodeState OnUpdate()
    {
        return NodeState.SUCCESS;
    }
}