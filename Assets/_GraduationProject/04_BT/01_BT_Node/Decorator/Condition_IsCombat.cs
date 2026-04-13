using BehaviorTree;
using UnityEngine;

/// <summary>
/// 마지막 공격 성공 시각과 마지막 피격 시각을 기준으로 전투 이탈 여부를 판정합니다.
/// </summary>
public class Condition_IsCombat : ConditionNode
{
    /// <summary>
    /// 마지막 상호작용 이후 이 시간(초)이 지나면 true를 반환합니다.
    /// </summary>
    [Header("전투 이탈 판정")]
    [Tooltip("마지막 공격/피격 시간 이후 true로 전환되기까지의 시간(초)")]
    [Min(0f)]
    public float noCombatDuration = 3f;

    private bool _wasIdleEnough;

    /// <summary>
    /// 노드 진입 시 상태를 초기화합니다.
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();
        _wasIdleEnough = false;
    }

    protected override bool CheckCondition()
    {
        if (runner == null || brain == null || brain.blackboard == null)
        {
            return false;
        }

        if (!brain._isCombat)
        {
            _wasIdleEnough = false;
            return false;
        }

        float lastAttackSuccessTime = brain.blackboard.GetValueOrDefault<float>(EnemyBlackboardKeys.LastAttackSuccessTime, -1f);
        float lastTakeHitTime = brain.blackboard.GetValueOrDefault<float>(EnemyBlackboardKeys.LastTakeHitTime, -1f);
        float latestInteractionTime = Mathf.Max(lastAttackSuccessTime, lastTakeHitTime);

        if (latestInteractionTime < 0f)
        {
            _wasIdleEnough = false;
            return false;
        }

        bool isIdleEnough = (Time.time - latestInteractionTime) >= noCombatDuration;
        if (isIdleEnough && !_wasIdleEnough)
        {
            BTDebug.Log($"[Condition_IsCombat] {runner.name} 전투이탈 판정: 최근 상호작용 후 {noCombatDuration:F2}초 경과");
        }

        _wasIdleEnough = isIdleEnough;
        return isIdleEnough;
    }

    /// <summary>
    /// 런타임 복제본을 생성합니다.
    /// </summary>
    public override Node Clone()
    {
        return Instantiate(this);
    }
}
