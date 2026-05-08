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
        int phase = 1;

        if (healthPercent <= 0.5f)
        {
            Debug.Log($"healthPercent: {healthPercent} runner.EnemyHealth.CurrentHealth: {runner.EnemyHealth.CurrentHealth} runner.EnemyHealth.MaxHealth: {runner.EnemyHealth.MaxHealth}    ");   
            phase = 2;
        }
        // if (healthPercent <= 0.25f)
        // {
        //     phase = 2;
        // }
        if(runner._aiController._aiBrain.blackboard.GetValue<int>(EnemyBlackboardKeys.Phase) != phase)
            runner._aiController._aiBrain.blackboard.SetValue(EnemyBlackboardKeys.Phase, phase);
    }
}
