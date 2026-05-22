using BehaviorTree;

public class Condition_CameraPublish : ConditionNode
{
    public EnemyStateType stateType;
    protected override bool CheckCondition()
    {
        return stateType == runner.StateType;
    }
    public override Node Clone()
    {
        Condition_CameraPublish node = Instantiate(this);
        return node;
    }


}