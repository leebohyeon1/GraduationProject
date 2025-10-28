using UnityEngine;
using BehaviorTree;

public class SetAbility : Node
{
    public bool Ability;
    
    public override Node Clone()
    {
        SetAbility node = ScriptableObject.CreateInstance<SetAbility>();
        node.Ability = Ability;
        return node;
    }

    public override void OnEnter()
    {
        Debug.Log("Set Ability Node: " + Ability);
        runner.specialAbility.SetAbility(Ability);
        runner.EnemyHealth.SetKnockbackable(false);

    }

    protected override NodeState OnUpdate()
    {
        return NodeState.SUCCESS;
    }
}