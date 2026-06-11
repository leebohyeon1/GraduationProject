using UnityEngine;
using BehaviorTree;

[CreateAssetMenu(fileName = "Action_ClearStun", menuName = "BehaviorTree/Action/ClearStun")]
public class Action_ClearStun : Node
{
    protected override NodeState OnUpdate()
    {
        if (runner == null || runner.ParrySystem == null) return NodeState.FAILURE;
        Debug.Log($"[Action_ClearStun] Attempting to clear stun. Current Stun: {runner.ParrySystem.CurrentStun}, Is Stunned: {runner.ParrySystem._isStunned}");
        runner.ParrySystem.ClearStun();
        return NodeState.SUCCESS;
    }

    public override Node Clone() => Instantiate(this);
}
