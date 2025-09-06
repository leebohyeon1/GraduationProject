using UnityEngine;

/// <summary>
/// 이동을 담당하는 인터페이스입니다.
/// </summary>
public interface IMovable
{
    /// <summary>
    /// 현재 이동 속도
    /// </summary>
    public float MoveSpeed { get; }

    /// <summary>
    /// 이동 방향으로 움직이는 함수
    /// </summary>
    /// <param name="direction">이동할 방향</param>
    /// <param name="speed">이동 속도</param>
    /// <param name="speedMultiplier">속도 배율 (기본값: 1f)</param>
    public void Move(Vector3 direction, float speed, float speedMultiplier = 1f);
}