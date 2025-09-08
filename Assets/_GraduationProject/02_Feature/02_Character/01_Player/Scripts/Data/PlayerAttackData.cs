using UnityEngine;

/// <summary>
/// 플레이어 근접 공격 관련 데이터
/// 공격 시 전진 이동, 공격력, 범위 등을 정의
/// </summary>
[System.Serializable]
public class PlayerMeleeAttackData
{
    [Header("Attack Movement")]
    [Tooltip("공격 시 전진할 거리")]
    public float AttackMoveDistance = 2f;

    [Tooltip("전진 이동 지속 시간")]
    public float AttackMoveDuration = 0.3f;

    [Tooltip("전진 이동 애니메이션 곡선")]
    public AnimationCurve AttackMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Attack Stats")]
    [Tooltip("공격 데미지")]
    public int AttackDamage = 10;

    [Tooltip("공격 범위 반지름")]
    public Vector3 AttackRadius = Vector3.one;

    [Header("Attack Timing")]
    [Tooltip("공격후 딜레이 시간")]
    public float AttackDelay = 0.2f;
}

/// <summary>
/// 플레이어 원거리 공격 관련 데이터
/// </summary>
[System.Serializable]
public class RangedAttackData
{
    [Header("Charge Stats")]
    public float RangedAttackChargeTime;

    [Header("Attack Stats")]
    public int AttackDamage = 10;

    [Header("Projectile")]
    public float ProjectileSpeed;
    public AnimationCurve ProjectileMoveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

}