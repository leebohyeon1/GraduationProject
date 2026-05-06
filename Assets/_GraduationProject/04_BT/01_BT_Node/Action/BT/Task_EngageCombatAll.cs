using UnityEngine;
using BehaviorTree;

public class Task_EngageCombatAll : Node
{

    public override void OnEnter()
    {
        
        if (!brain._isCombat)
        {
            runner._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.Engage, true);
            runner.groupAi.EngageCombatAll();
            runner._aiController.CombatEnter();
        }
    }

    protected override NodeState OnUpdate()
    {
        return NodeState.SUCCESS;
    }



    public override Node Clone()
    {
        Task_EngageCombatAll node = Instantiate(this);
        return node;
    }
}
