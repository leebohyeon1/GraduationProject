using System;

/// <summary>
/// 플레이어 체력 시스템 인터페이스
/// IDamageable과 IHealable을 상속받아 플레이어 전용 체력 기능을 추가 정의합니다.
/// </summary>
public interface IPlayerHealth : IDamageable, IHealable
{
    /// <summary>피격 상태 플래그를 리셋 (Hit 상태 종료 시 호출)</summary>
    void ResetHitState();
    
    /// <summary>방어 상태 설정</summary>
    void SetDefending(bool isDefending);
    
    /// <summary>생존 여부</summary>
    bool IsAlive { get; }
    
    /// <summary>피격 상태 여부 (상태 머신 전환 조건)</summary>
    bool IsHit { get; }
}
