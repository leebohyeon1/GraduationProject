using UnityEngine;
using BehaviorTree;
public class Service_UpdateCombatVars : ServiceNode
{
    public GameObject CurrentTarget;
    public string DistanceToTarget = "DistanceToTarget";
    public string LocationKey = "TargetLocation";
    public override void OnEnter()
    {
        base.OnEnter();
        if(CurrentTarget == null)
        {
            CurrentTarget = runner.player.gameObject;
        }
    }
    protected override void OnServiceLogic()
    {
        float distance = Vector3.Distance(runner.transform.position, CurrentTarget.transform.position);
        runner._aiController._aiBrain.blackboard.SetValue(DistanceToTarget, distance);
        runner._aiController._aiBrain.blackboard.SetValue(LocationKey, CurrentTarget.transform.position);
    }
}
