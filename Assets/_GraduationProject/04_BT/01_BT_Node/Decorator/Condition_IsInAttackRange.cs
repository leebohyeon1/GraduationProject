using UnityEngine;
using BehaviorTree;

public class Condition_IsInAttackRange : ConditionNode
{
    [Tooltip("AI가 플레이어를 공격할 수 있는 범위")]
    [SerializeField] float _attackRange = 2f;    public override void OnEnter()
    {
        base.OnEnter();
    }
    protected override bool CheckCondition()
    {
        return runner != null && brain.IsInAttackRange(_attackRange);
    }
    public override Node Clone()
    {
        Condition_IsInAttackRange clone = Instantiate(this);
        clone._attackRange = this._attackRange;
        return clone;
    }
}
