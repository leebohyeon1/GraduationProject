using UnityEngine;
using BehaviorTree;

public enum ComparisonOperator { Less, LessOrEqual, Equal, NotEqual, GreaterOrEqual, Greater }
public enum StateOperator { Equal, NotEqual }

[System.Serializable]
public class FloatCondition
{
    public string key;
    public ComparisonOperator Operator;
    public float value;

    public bool isCondition(BlackBoard blackboard)
    {
        if (!blackboard.GetValue<float>(key, out float actualValue)) return false;

        switch (Operator)
        {
            case ComparisonOperator.Less: return actualValue < value;
            case ComparisonOperator.LessOrEqual: return actualValue <= value;
            case ComparisonOperator.Equal: return actualValue == value;
            case ComparisonOperator.NotEqual: return actualValue != value;
            case ComparisonOperator.GreaterOrEqual: return actualValue >= value;
            case ComparisonOperator.Greater: return actualValue > value;
            default: return false;
        }
    }
}

[System.Serializable]
public class StateCondition
{
    public string Key = "CurrentStatus";
    public StateOperator Operator;
    public EnemyStateController.EnemyState TargetState;

    public bool isCondition(BlackBoard blackboard)
    {
        if (!blackboard.GetValue<EnemyStateController.EnemyState>(Key, out EnemyStateController.EnemyState actualState)) return false;

        switch (Operator)
        {
            case StateOperator.Equal: return actualState == TargetState;
            case StateOperator.NotEqual: return actualState != TargetState;
            default: return false;
        }
    }
}

[System.Serializable]
public class ExceptCondition
{
    public string key;
    public bool isCondition(BlackBoard blackboard) => blackboard.GetValue<bool>(key);
}
