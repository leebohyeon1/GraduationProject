using BehaviorTree;
using UnityEngine;

public class Condition_Heighdiff : ConditionNode
{
    [Tooltip("공격 가능한 높이 차 (이상일 경우 공격 시도하지 않음)")]
    [SerializeField] private float _maxHeightDifference = 3.0f;
   
    public override Node Clone()
    {
        Condition_Heighdiff clone = ScriptableObject.CreateInstance<Condition_Heighdiff>();
        clone._maxHeightDifference = this._maxHeightDifference;
        return clone;
    }

    protected override bool CheckCondition()
    {
         float heightdiff = runner.transform.position.y - runner.player.transform.position.y;
        float absHeightDiff = Mathf.Abs(heightdiff);
        // _maxHeightDifference보다 높이 차이가 크면 공격 시도하지 않음
        if(absHeightDiff > _maxHeightDifference)
        {
            // Debug.Log($"[Condition_Heighdiff] Height difference {absHeightDiff} exceeds max {_maxHeightDifference}. Condition failed.");
            return false;
        }
        else
        {
            return true;
        }
    }
}