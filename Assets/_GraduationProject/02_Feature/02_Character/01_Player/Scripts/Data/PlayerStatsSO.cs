using UnityEngine;

/// <summary>
/// 플레이어 전용 스탯 정의
/// CharacterStats를 상속받아 플레이어만의 고유한 능력치를 추가
/// </summary>
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/Player Stats")]
public class PlayerStatsSO : CharacterStatsSO
{
    [Header("Movement")]
    public float RotateSpeed = 5f;

    [Header("Physics")]
    public float Gravity = -9.81f;
    public float GroundCheckDistance = 0.1f;

    [Header("Player Combat")]
    public PlayerMeleeAttackData[] AttackData;
    public RangedAttackData RangedAttackData;
    public Vector3 ParryRadius;

    [Header("Player Dodge")]
    public float DodgeSpeed = 8f;
    public float DodgeCooldown = 2f;

    [Header("Player Damaged")]
    public float DefendDamageReductionRate = 0.7f; // 방어 시 데미지 감소율
    public float HitStunDuration = 0.1f; // 피격 시 멈춤 시간
}
