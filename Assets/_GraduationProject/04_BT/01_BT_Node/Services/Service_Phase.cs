using UnityEngine;
using BehaviorTree;
public class Service_Phase : ServiceNode
{
    protected override void OnServiceLogic()
    {
        if (!runner._aiController._aiBrain._isCombat)
        {
            runner._aiController._aiBrain.blackboard.SetValue("Phase", 0);
            return;
        }
        float healthPercent = runner.EnemyHealth.CurrentHealth / runner.EnemyHealth.MaxHealth;
        int phase = 0;
        if (healthPercent <= 0.5f)
        {
            phase = 1;
        }
        if (healthPercent <= 0.25f)
        {
            phase = 2;
        }
        runner._aiController._aiBrain.blackboard.SetValue("Phase", phase);
    }
}
