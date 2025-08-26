using UnityEngine;

public interface IPlayerMovement: IMovable
{
    void Dodge(Vector3 direction, bool hasInput);
    void RotateImmediately(Vector3 direction);
    bool CanDodge();
    void Tick();
    System.Collections.IEnumerator CoMoveForwardWithCurve(float distance, float duration, AnimationCurve curve);
    bool IsGrounded { get; }
    Vector3 Velocity { get; }
    float DodgeSpeed { get; }
}
