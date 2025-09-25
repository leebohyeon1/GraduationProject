using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "IsInAttackRangeCondition", menuName = "BehaviorTree/Condition/IsInAttackRange")]
public class Condition_IsInAttackRange : ConditionNode
{
    public bool heatRange = false;
    [Tooltip("The range within which the AI can attack the player")]
    [SerializeField] float _attackRange = 2f;
    CalculationResult stat;
    public override void OnEnter()
    {
        base.OnEnter();
        stat = runner.heatSystem.CalculationHeat("Test", ActorType.Monster, runner.heatSystem.GetTier(), 0);
    }
    protected override bool CheckCondition()
    {
        return runner != null && brain.IsInAttackRange(_attackRange * stat.FinalRange);
    }
    public override Node Clone()
    {
        Condition_IsInAttackRange clone = Instantiate(this);
        clone.heatRange = this.heatRange;
        clone._attackRange = this._attackRange;
        return clone;
    }
}
