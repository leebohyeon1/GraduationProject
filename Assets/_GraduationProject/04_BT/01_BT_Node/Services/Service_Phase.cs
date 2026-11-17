using UnityEngine;
using BehaviorTree;
public class Service_Phase : ServiceNode
{
    public override Node Clone()
    {
        return Instantiate(this);
    }

    protected override void OnServiceLogic()
    {
        if (!runner._aiController._aiBrain._isCombat)
        {
            runner._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.Phase, 0);
            return;
        }
        float healthPercent = (float)runner.EnemyHealth.CurrentHealth / (float)runner.EnemyHealth.MaxHealth;
        int phase = Mathf.Max(1, runner._aiController._aiBrain.blackboard.GetValueOrDefault<int>(EnemyBlackboardKeys.Phase, 0));

        if (healthPercent <= 0.5f)
        {
            Debug.Log($"healthPercent: {healthPercent} runner.EnemyHealth.CurrentHealth: {runner.EnemyHealth.CurrentHealth} runner.EnemyHealth.MaxHealth: {runner.EnemyHealth.MaxHealth}    ");   
            runner._aiController._aiBrain.blackboard.SetValue("RequestedPhase", 2f);
            if (runner._aiController._aiBrain.blackboard.GetValueOrDefault<bool>("Phase_2", false)) phase = 2;
        }
        // if (healthPercent <= 0.25f)
        // {
        //     runner._aiController._aiBrain.blackboard.SetValue("RequestedPhase", 2f);
            if (runner._aiController._aiBrain.blackboard.GetValueOrDefault<bool>("Phase_2", false)) phase = 2;
        // }
        if(runner._aiController._aiBrain.blackboard.GetValueOrDefault<int>(EnemyBlackboardKeys.Phase, 0) != phase)
            runner._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.Phase, phase);

        Debug.Log($"[Service_Phase] Health Percent: {healthPercent}, Current Phase: {runner._aiController._aiBrain.blackboard.GetValueOrDefault<int>(EnemyBlackboardKeys.Phase, 0)}");
    }
}
