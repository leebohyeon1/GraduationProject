using System;
using UnityEngine;

/// <summary>
/// 스텝 데이터
/// </summary>
[Serializable]
public struct StepData
{
    public float StepDistance;  // 스텝 거리
    public float StepDuration;  // 스텝 시간
    public AnimationCurve StepCurve;    // 스텝 곡선

    public float StepRotateSpeed;   // 스텝 중 회전 속도
}
