using System;
using UnityEngine;

/// <summary>
/// 경직도를 가지고 있는 오브젝트에 넣는 인터페이스
/// </summary>
public interface IStiffness
{
    /// <summary>
    /// 현재 경직도
    /// </summary>
    int CurrentStiffness { get; }

    /// <summary>
    /// 경직 임계점
    /// </summary>
    int StiffnessThreshold { get; }

    /// <summary>
    /// 경직 지속 시간
    /// </summary>
    float StiffnessDuration { get; }
    
    /// <summary>
    /// 경직도 게이지 누적
    /// </summary>
    /// <param name="amount"></param>
    void AddStiffness(int amount);
    
    event Action<int, int> OnStiffnessChanged;   
} 
