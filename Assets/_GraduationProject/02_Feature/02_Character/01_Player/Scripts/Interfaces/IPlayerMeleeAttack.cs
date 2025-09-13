using UnityEngine;

/// <summary>
/// 플레이어 공격 시스템 인터페이스
/// IAttacker를 상속받아 플레이어 전용 공격 기능을 추가 정의합니다.
/// </summary>
public interface IPlayerMeleeAttack: IAttacker
{
    /// <summary>
    /// 실제 공격 실행 (피해 계산 및 적용)
    /// </summary>
    void PerformAttack();
    
    /// <summary>
    /// 콤보 카운트 리셋
    /// </summary>
    void ResetComboCount();
    
    /// <summary>
    /// 공격 범위의 중심점 계산
    /// </summary>
    void SetAttackCenter();

    /// <summary>
    /// 근거리 공격 데이터
    /// </summary>
    PlayerMeleeAttackData MeleeAttackData { get; }
    
    Vector3 AttackCenter { get; }
    
    /// <summary>
    /// 현재 콤보 카운트
    /// </summary>
    int ComboCount { get; }
}
