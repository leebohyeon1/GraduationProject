using UnityEngine;
using BehaviorTree;
using System;
[CreateAssetMenu(fileName = "Condition_OpportunityFinder", menuName = "BehaviorTree/Condition/OpportunityFinder")]
public class Condition_OpportunityFinder : ConditionNode
{
    [SerializeField]
    public bool PlayerAttack;
    
    public override Node Clone()
    {
        var node = Instantiate(this);
        node.PlayerAttack = this.PlayerAttack;
        return node;
    }

    protected override bool CheckCondition()
    {
        // if(runner.player.)
        throw new System.NotImplementedException();
    }

}
