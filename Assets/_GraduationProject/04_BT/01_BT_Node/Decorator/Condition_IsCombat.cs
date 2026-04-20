using BehaviorTree;
using UnityEngine;

/// <summary>
/// 留덉?留?怨듦꺽 ?깃났 ?쒓컖怨?留덉?留??쇨꺽 ?쒓컖??湲곗??쇰줈 ?꾪닾 ?댄깉 ?щ?瑜??먯젙?⑸땲??
/// </summary>
public class Condition_IsCombat : ConditionNode
{
    /// <summary>
    /// 留덉?留??곹샇?묒슜 ?댄썑 ???쒓컙(珥???吏?섎㈃ true瑜?諛섑솚?⑸땲??
    /// </summary>
    [Header("?꾪닾 ?댄깉 ?먯젙")]
    [Tooltip("留덉?留?怨듦꺽/?쇨꺽 ?쒓컙 ?댄썑 true濡??꾪솚?섍린源뚯????쒓컙(珥?")]
    [Min(0f)]
    public float noCombatDuration = 3f;

    private bool _wasIdleEnough;

    /// <summary>
    /// ?몃뱶 吏꾩엯 ???곹깭瑜?珥덇린?뷀빀?덈떎.
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
        }

        _wasIdleEnough = isIdleEnough;
        return isIdleEnough;
    }

    /// <summary>
    /// ?고???蹂듭젣蹂몄쓣 ?앹꽦?⑸땲??
    /// </summary>
    public override Node Clone()
    {
        return Instantiate(this);
    }
}
