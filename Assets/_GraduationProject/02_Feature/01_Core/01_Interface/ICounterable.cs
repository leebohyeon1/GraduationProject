using UnityEngine;

/// <summary>
/// 카운터 공격 가능 여부를 나타내는 인터페이스
/// </summary>
public interface ICounterable
{
    /// <summary>
    /// 카운터 공격 가능 여부
    /// </summary>
    bool IsCounterable { get; }

    /// <summary>
    /// 카운터 효과 실행
    /// </summary>
    void ExecuteCounterEffect();
}
