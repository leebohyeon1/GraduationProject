using UnityEngine;

/// <summary>
/// 플레이어 이동 시스템 인터페이스
/// IMovable을 상속받아 플레이어 전용 이동 기능을 추가 정의합니다.
/// </summary>
public interface IPlayerMovement: IMovable
{
    /// <summary>회피 이동 실행</summary>
    void Dodge(Vector3 direction, bool hasInput);
    
    /// <summary>지정된 방향으로 즉시 회전</summary>
    void RotateImmediately(Vector3 direction);
    
    /// <summary>회피 가능 여부 (쿨다운 체크)</summary>
    bool CanDodge();
    
    /// <summary>물리 업데이트 (중력 적용 등)</summary>
    void Tick();
    
    /// <summary>곡선을 사용한 전진 이동 코루틴 (공격 시 사용)</summary>
    System.Collections.IEnumerator CoMoveForwardWithCurve(float distance, float duration, AnimationCurve curve);
    
    /// <summary>지면 접촉 여부</summary>
    bool IsGrounded { get; }
    
    /// <summary>현재 속도 벡터</summary>
    Vector3 Velocity { get; }
    
    /// <summary>회피 속도</summary>
    float DodgeSpeed { get; }
}
