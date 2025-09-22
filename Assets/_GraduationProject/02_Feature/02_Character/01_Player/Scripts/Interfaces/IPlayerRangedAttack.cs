using UnityEngine;

/// <summary>
/// 플레이어 원거리 공격 시스템 인터페이스
/// 투사체를 사용한 원거리 공격 기능을 정의합니다.
/// </summary>
public interface IPlayerRangedAttack
{
    /// <summary>원거리 공격 차징 시간</summary>
    public float RangedAttackChargeTime { get; }
    
    /// <summary>원거리 공격 데미지</summary>
    public int RangedAttackDamage { get; }

    /// <summary>투사체 발사 실행</summary>
    public void FireProjectile();
}
