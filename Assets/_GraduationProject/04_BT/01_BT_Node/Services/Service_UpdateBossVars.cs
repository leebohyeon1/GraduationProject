using UnityEngine;
using System.Collections.Generic;
using BehaviorTree;
using System;

/// <summary>
/// ?뱀젙 援ъ뿭?ㅼ쓣 媛먯떆?섏뿬 ?뚮젅?댁뼱媛 ?덉쟾 援ъ뿭??癒몃Т???쒓컙???꾩쟻?⑸땲??
/// 由ъ뀑 援ъ뿭???ㅼ뼱媛嫄곕굹 寃쎄퀎瑜?踰쀬뼱?섏? ?딅뒗 ???쒓컙? 怨꾩냽 ?꾩쟻?⑸땲??
/// </summary>
[CreateAssetMenu(fileName = "Service_UpdateBossVars", menuName = "BehaviorTree/Service/UpdateBossVars")]
public class Service_UpdateBossVars : ServiceNode
{
    [Header("Zone IDs")]
    [Tooltip("플레이어가 머무를 수 있는 경계 구역 ID")]
    public int BoundaryZoneID = 3;

    [Tooltip("진입 시 즉시 누적 시간을 0으로 초기화할 구역 ID 목록")]
    public List<int> ResetZoneIDs = new List<int>();

    [Header("Timing Settings")]
    [Tooltip("목표 누적 시간(초)")]
    public float ThresholdTime = 5.0f;

    [Header("Blackboard Keys")]
    public string ResultKey = "TargetZoneCheck";

    private float _accumulatedTime = 0f;

    public override void initNode()
    {
        _accumulatedTime = 0f;
        brain.blackboard.SetValue(ResultKey, false);

    }
    protected override void OnServiceLogic()
    {
        if (runner == null || runner.player == null) return;
        var tracker = runner.player.GetComponent<PlayerZoneTracker>();
        if (tracker == null) return;

        // 1. 珥덇린??Reset) 議곌굔 泥댄겕
        // - 寃쎄퀎 援ъ뿭(Boundary)???꾩쟾??踰쀬뼱?щ뒗媛?
        bool isOutsideBoundary = !tracker.IsInZone(BoundaryZoneID);
        
        // - 紐낆떆?곸씤 珥덇린??援ъ뿭(ResetZone)???ㅼ뼱?붾뒗媛?
        bool isInResetZone = false;
        foreach (int id in ResetZoneIDs)
        {
            if (tracker.IsInZone(id)) { isInResetZone = true; break; }
        }

        // 珥덇린??議곌굔(寃쎄퀎 諛??뱀? 由ъ뀑 援ъ뿭 ?????대떦?섎㈃ ?쒓컙??0?쇰줈 留뚮뱾怨?醫낅즺
        if (isOutsideBoundary || isInResetZone)
        {
            // Debug.Log($"[Service_UpdateBossVars] Player is outside boundary or in reset zone. Resetting accumulated time. (OutsideBoundary: {isOutsideBoundary}, IsInResetZone: {isInResetZone})");
            if (_accumulatedTime > 0)
            {
                initNode();

            }
            return;
        }

        // 2. ?쒓컙 ?꾩쟻 (珥덇린??援ш컙???꾨땲?쇰㈃ 臾댁“嫄??꾩쟻)
        // ?뚮젅?댁뼱媛 Boundary 援ъ뿭 ?덉뿉 ?덇퀬 Reset 援ъ뿭???덉? ?딅떎硫?臾댁“嫄??쒓컙???볦엯?덈떎.
        _accumulatedTime += UpdateInterval;

        Debug.Log(_accumulatedTime);
        // 3. ?꾧퀎移??꾨떖 ??釉붾옓蹂대뱶 ?뚮옒洹??쒖꽦??
        if (_accumulatedTime >= ThresholdTime)
        {
            if (!brain.blackboard.GetValueOrDefault<bool>(ResultKey, false))
            {
                Debug.Log($"[Service_UpdateBossVars] Player has been in the target zone for {_accumulatedTime} seconds. Setting {ResultKey} to true.");
                brain.blackboard.SetValue(ResultKey, true);
                runner.Movement.StopMovement();
            }
        }
    }

    public override Node Clone()
    {
        Service_UpdateBossVars newNode = Instantiate(this);
        newNode.ResetZoneIDs = new List<int>(ResetZoneIDs);
        return newNode;
    }
}
