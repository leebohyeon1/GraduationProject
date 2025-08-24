using UnityEngine;

/// <summary>
/// 넉백 효과를 받을 수 있는 오브젝트를 위한 인터페이스입니다.
/// </summary>
public interface IKnockbackable
{
    /// <summary>
    /// 넉백이 가능한지 여부입니다.
    /// </summary>
    public bool IsKnockbackable { get; }

    /// <summary>
    /// 오브젝트에 넉백을 적용합니다.
    /// </summary>
    /// <param name="knockbackForce">넉백의 힘(크기)입니다.</param>
    /// <param name="direction">넉백 방향.</param>
    public void ApplyKnockback(float knockbackForce, Vector3 direction);
}
