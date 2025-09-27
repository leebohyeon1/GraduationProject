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
        if (typeof(PlayerAttackBaseState).IsAssignableFrom(runner.player.CurrentPlayerState))
        {
            return true;
        }
        else if (runner.player.CurrentPlayerState == typeof(PlayerDodgeState))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
