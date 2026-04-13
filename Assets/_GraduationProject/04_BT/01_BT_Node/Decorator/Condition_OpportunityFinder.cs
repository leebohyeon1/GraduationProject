using UnityEngine;
using BehaviorTree;
using System;

public class Condition_OpportunityFinder : ConditionNode
{

    
    public override Node Clone()
    {
        var node = Instantiate(this);
        return node;
    }

    protected override bool CheckCondition()
    {
        if (typeof(PlayerAttackBaseState).IsAssignableFrom(runner.player.FSM.CurrentState.GetType()))
        {
            // // BTDebug.Log(runner.player.FSM.CurrentState);
            return true;
        }
        else if (runner.player.FSM.CurrentState.GetType() == typeof(PlayerDodgeState))
        {
            // // BTDebug.Log(runner.player.FSM.CurrentState);
            return true;
        }
        else
        {
            return false;
        }
    }

}
