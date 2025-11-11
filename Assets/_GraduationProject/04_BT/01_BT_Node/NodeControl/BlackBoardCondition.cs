using UnityEngine;
using BehaviorTree;
public enum ComparisonOperator
{
    Less,          // < (작다)
    LessOrEqual,   // <= (작거나 같다)
    Equal,         // == (같다)
    NotEqual,      // != (다르다)
    GreaterOrEqual, // >= (크거나 같다)
    Greater        // > (크다)
}

[System.Serializable]
public class FloatCondition
{
    [Tooltip("비교할 블랙보드 키")]
    public string key;
    [Tooltip("비교할 방법")]
    public ComparisonOperator Operator;
    [Tooltip("비교할 값")]
    public float value;

    public bool isCondition(BlackBoard blackboard)
    {
        if (!blackboard.GetValue<float>(key, out float actualValue))
        {
            // 키에 해당하는 값이 블랙보드에 없거나 float 타입이 아님
            Debug.LogWarning($"[BlackboardCondition] 키 '{key}'를 찾을 수 없거나 타입이 float이 아닙니다.");
            return false;
        }

        // 2. Enum 값에 따라 실제 C# 연산자를 사용해 비교
        switch (Operator)
        {
            case ComparisonOperator.Less:
                return actualValue < value;
            case ComparisonOperator.LessOrEqual:
                return actualValue <= value;
            case ComparisonOperator.Equal:
                // float 비교는 근사치로 하는 것이 좋지만, 
                // 여기서는 편의상 == 로 하겠습니다.
                return actualValue == value;
            case ComparisonOperator.NotEqual:
                return actualValue != value;
            case ComparisonOperator.GreaterOrEqual:
                return actualValue >= value;
            case ComparisonOperator.Greater:
                return actualValue > value;
            default:
                return false;
        }
    }
}
[System.Serializable]
public class StateCondition
{
    [Tooltip("비교할 블랙보드의 키 (예: CurrentState)")]
    public string Key = "CurrentState"; // 기본값으로 설정

    [Tooltip("'Is' (같다) 또는 'IsNot' (다르다)")]
    public ComparisonOperator Operator;

    [Tooltip("비교할 대상 상태")]
    public Enemy.EnemyState TargetState; // Enemy.cs 안에 정의된 EnemyState Enum

    public bool isCondition(BlackBoard blackboard)
    {
        // 1. 블랙보드에서 "CurrentState" 키로 값을 가져옵니다.
        //    (타입을 Enemy.EnemyState로 요청)
        if (!blackboard.GetValue<Enemy.EnemyState>(Key, out Enemy.EnemyState actualState))
        {
            // 키가 없거나 타입이 다르면 실패
            Debug.LogWarning($"[StateCondition] 키 '{Key}'를 찾을 수 없거나 타입이 Enemy.EnemyState가 아닙니다.");
            return false;
        }

        // 2. Operator에 따라 실제 상태(actualState)와 목표 상태(TargetState)를 비교
        switch (Operator)
        {
            case ComparisonOperator.Equal:
                return actualState == TargetState; // 예: (현재 상태) == (Idle)
            
            case ComparisonOperator.NotEqual:
                return actualState != TargetState; // 예: (현재 상태) != (Die)
            
            default:
                return false;
        }
    }
}