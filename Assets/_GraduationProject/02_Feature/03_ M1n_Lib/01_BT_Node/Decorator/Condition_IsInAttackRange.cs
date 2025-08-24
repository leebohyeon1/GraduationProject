using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "IsInAttackRangeCondition", menuName = "BehaviorTree/Condition/IsInAttackRange")]
public class Condition_IsInAttackRange : ConditionNode
{
    public override Node Clone() => Instantiate(this);
    [Tooltip("The range within which the AI can attack the player")]
    [SerializeField] float _attackRange = 2f;

    protected override bool CheckCondition()
    {
        return runner != null && brain.IsInAttackRange(_attackRange);
    }
}
