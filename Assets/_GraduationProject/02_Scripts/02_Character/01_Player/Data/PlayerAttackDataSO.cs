using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAttackDataSO", menuName = "Player/AttackDatasSO")]
public class PlayerAttackDataSO : ScriptableObject
{
    public float AttackStamina;

    [Header("Attack Movement")]
    [Tooltip("공격 시 전진하는 거리")]
    public float AttackMoveDistance;

    [Tooltip("공격 이동에 걸리는 시간")]
    public float AttackMoveDuration;

    [Tooltip("공격 이동 애니메이션 커브")]
    public AnimationCurve AttackMoveCurve;

    [Header("Attack Stats")]
    [Tooltip("공격 데미지")]
    public int AttackDamage;

    [Tooltip("공격 범위")]
    public Vector3 AttackRadius;

    [Header("Knockback")]
    public AnimationCurve KnockBackCurve;
    public float KnockBackDuration;
    public float KnockBackForce;
}
