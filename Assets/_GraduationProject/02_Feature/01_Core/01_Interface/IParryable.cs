using UnityEngine;

public interface IParryable
{
    /// <summary>
    /// 현재 패링이 가능한 상태인지 여부를 나타냅니다.
    /// </summary>
    public bool IsParryable { get; }

    /// <summary>
    /// 이 오브젝트에 대한 패링을 시도합니다.
    /// </summary>
    /// <param name="parryInstigator">패링을 시도한 주체 (예: 플레이어)의 GameObject입니다.</param>
    /// <returns>패링 성공 여부를 반환합니다.</returns>
    public bool Parry(GameObject parryInstigator);
}
