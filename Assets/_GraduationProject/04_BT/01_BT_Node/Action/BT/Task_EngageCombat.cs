using UnityEngine;
using BehaviorTree;

public class Task_EngageCombat : Node
{

    public override void OnEnter()
    {
        
        if (!brain._isCombat)
        {
            runner._aiController.CombatEnter();
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
