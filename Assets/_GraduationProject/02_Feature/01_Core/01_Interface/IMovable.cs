using UnityEngine;

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
    public void Move(Vector3 direction);
}