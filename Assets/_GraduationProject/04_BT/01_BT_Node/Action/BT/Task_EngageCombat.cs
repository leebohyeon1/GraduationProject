using UnityEngine;
using BehaviorTree;

public class Task_EngageCombat : Node
{

    public override void OnEnter()
    {
        // Debug.Log("[Task_EngageCombat : " + runner.name + "] 전투 돌입.");
        
        if (!brain._isCombat)
        {
            runner._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.Engage, true);
            runner.groupAi.EngageCombatAll();
        }
    }

    protected override NodeState OnUpdate()
    {
        return NodeState.SUCCESS;
    }



    public override Node Clone()
    {
        Task_EngageCombat node = Instantiate(this);
        return node;
    }
}
