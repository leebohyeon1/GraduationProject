using UnityEngine;

public interface IStunable
{
    
    /// <summary>
    /// 현재 스턴이 가능한 상태인지 여부를 나타냅니다.
    /// </summary>
    bool IsStunable { get; }

    /// <summary>
    /// 이 오브젝트에 스턴을 시도합니다.
    /// </summary>
    /// <param name="stunDuration">스턴 지속 시간(초)입니다.</param>
    /// <param name="stunInstigator">스턴을 시도한 주체 (예: 플레이어)의 GameObject입니다.</param>
    /// <returns>스턴 성공 여부를 반환합니다.</returns>
    bool Stun(float stunDuration, GameObject stunInstigator);
}
