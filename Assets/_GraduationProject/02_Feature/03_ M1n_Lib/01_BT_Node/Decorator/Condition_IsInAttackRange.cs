using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "IsInAttackRangeCondition", menuName = "BehaviorTree/Condition/IsInAttackRange")]
public class Condition_IsInAttackRange : ConditionNode
{
    public bool heatRange = false;
    [Tooltip("AI가 플레이어를 공격할 수 있는 범위")]
    [SerializeField] float _attackRange = 2f;
    CalculationResult stat;
    public override void OnEnter()
    {
        base.OnEnter();
        stat = runner.heatSystem.CalculationHeat("Test", ActorType.Monster, runner.heatSystem.GetTier(), 0);
    }
    protected override bool CheckCondition()
    {
        if (heatRange)
        {
            stat = runner.heatSystem.CalculationHeat("Test", ActorType.Monster, runner.heatSystem.GetTier(), 0);
            return runner != null && brain.IsInAttackRange(_attackRange * stat.FinalRange);
        }
        return runner != null && brain.IsInAttackRange(_attackRange);
    }
    public override Node Clone()
    {
        Condition_IsInAttackRange clone = Instantiate(this);
        clone.heatRange = this.heatRange;
        clone._attackRange = this._attackRange;
        return clone;
    }
}
