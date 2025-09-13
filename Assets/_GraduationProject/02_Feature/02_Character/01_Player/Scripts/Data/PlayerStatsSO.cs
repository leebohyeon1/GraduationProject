using UnityEngine;

/// <summary>
/// 플레이어 전용 스탯 정의
/// CharacterStatsSO를 상속받아 플레이어만의 고유한 능력치를 추가합니다.
/// Inspector에서 수정 가능한 ScriptableObject로 설계되었습니다.
/// </summary>
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/Player Stats")]
public class PlayerStatsSO : CharacterStatsSO
{
    [Header("Movement")]
    [Tooltip("회전 속도 (Slerp 보간 계수)")]
    public float RotateSpeed = 5f;

    [Header("Physics")]
    [Tooltip("중력 가속도 (음수 값)")]
    public float Gravity = -9.81f;
    [Tooltip("지면 체크 레이캐스트 거리")]
    public float GroundCheckDistance = 0.1f;

    [Header("Player Combat")]
    [Tooltip("근접 공격 데이터 배열 (콤보별 설정)")]
    public PlayerMeleeAttackData[] AttackData;

    [Tooltip("근거리 공격 차징 데이터")]
    public PlayerMeleeAttackData ChargeMeleeAttackData;
    public float MinChargeTime;

    [Tooltip("원거리 공격 데이터")]
    public RangedAttackData RangedAttackData;

    [Tooltip("패링 범위 (Box Collider 크기)")]
    public Vector3 ParryRadius;

    [Tooltip("카운터 공격 데이터")]
    public PlayerMeleeAttackData CounterAttackData;
    public float ParryCounterWindow = 0.5f;

    [Header("Player Dodge")]
    [Tooltip("회피 이동 속도")]
    public float DodgeSpeed = 8f;
    [Tooltip("회피 쿨다운 시간")]
    public float DodgeCooldown = 2f;

    [Header("Player Damaged")]
    [Tooltip("방어 시 데미지 감소율 (0.7 = 30%만 받음)")]
    public float DefendDamageReductionRate = 0.7f;
    [Tooltip("피격 시 경직 시간")]
    public float HitStunDuration = 0.1f;
}
