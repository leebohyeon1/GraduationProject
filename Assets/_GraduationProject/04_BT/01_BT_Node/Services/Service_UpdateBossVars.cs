using UnityEngine;
using System.Collections.Generic;
using BehaviorTree;
using System;

/// <summary>
/// 특정 구역들을 감시하여 플레이어가 안전 구역에 머무는 시간을 누적합니다.
/// 리셋 구역에 들어가거나 경계를 벗어나지 않는 한 시간은 계속 누적됩니다.
/// </summary>
[CreateAssetMenu(fileName = "Service_UpdateBossVars", menuName = "BehaviorTree/Service/UpdateBossVars")]
public class Service_UpdateBossVars : ServiceNode
{
    [Header("Zone IDs")]
    [Tooltip("플레이어가 활동 가능한 전체 영역 ID (이 밖으로 나가면 초기화)")]
    public int BoundaryZoneID = 3;

    [Tooltip("진입 시 즉시 누적 시간이 0으로 초기화되는 구역 ID 리스트")]
    public List<int> ResetZoneIDs = new List<int>();

    [Header("Timing Settings")]
    [Tooltip("목표 누적 시간 (초)")]
    public float ThresholdTime = 5.0f;

    [Header("Blackboard Keys")]
    public string ResultKey = "TargetZoneCheck";

    private float _accumulatedTime = 0f;
    public override void OnEnter()
    {
        base.OnEnter();
        _accumulatedTime = 0f;
    }
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

        // 1. 초기화(Reset) 조건 체크
        // - 경계 구역(Boundary)을 완전히 벗어났는가?
        bool isOutsideBoundary = !tracker.IsInZone(BoundaryZoneID);
        
        // - 명시적인 초기화 구역(ResetZone)에 들어왔는가?
        bool isInResetZone = false;
        foreach (int id in ResetZoneIDs)
        {
            if (tracker.IsInZone(id)) { isInResetZone = true; break; }
        }

        // 초기화 조건(경계 밖 혹은 리셋 구역 안)에 해당하면 시간을 0으로 만들고 종료
        if (isOutsideBoundary || isInResetZone)
        {
            if (_accumulatedTime > 0)
            {
                Debug.Log("리셋 조건 충족: " + (isOutsideBoundary ? "경계 밖" : "리셋 구역 진입") + ". 누적 시간 초기화.");
                initNode();

            }
            return;
        }

        // 2. 시간 누적 (초기화 구간이 아니라면 무조건 누적)
        // 플레이어가 Boundary 구역 안에 있고 Reset 구역에 있지 않다면 무조건 시간이 쌓입니다.
        _accumulatedTime += UpdateInterval;

        // 3. 임계치 도달 시 블랙보드 플래그 활성화
        if (_accumulatedTime >= ThresholdTime)
        {
            if (!brain.blackboard.GetValueOrDefault<bool>(ResultKey, false))
            {
                brain.blackboard.SetValue(ResultKey, true);
                Debug.Log("임계치 도달: " + _accumulatedTime + "초. 블랙보드 플래그 활성화.");
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