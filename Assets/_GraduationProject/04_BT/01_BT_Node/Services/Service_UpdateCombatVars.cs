using UnityEngine;
using BehaviorTree;
public class Service_UpdateCombatVars : ServiceNode
{
    public GameObject Target;
    public string DistanceToTarget = "Target";
    public string LocationKey = "TargetLocation";   
    protected override void OnServiceLogic()
    {
        float distance = Vector3.Distance(runner.transform.position, Target.transform.position);
        runner._aiController._aiBrain.blackboard.SetValue(DistanceToTarget, distance);
        runner._aiController._aiBrain.blackboard.SetValue(LocationKey, Target.transform.position);
    }
}
