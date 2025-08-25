using UnityEngine;

/// <summary>
/// 플레이어 공격 관련 데이터
/// 공격 시 전진 이동, 공격력, 범위 등을 정의
/// </summary>
[System.Serializable]
public class PlayerAttackData
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
    public float AttackRadius = 2f;
    
    [Header("Attack Timing")]
    [Tooltip("공격 쿨다운 시간")]
    public float AttackCooldown = 0.5f;
    
    [Tooltip("콤보 입력 허용 시간")]
    public float ComboInputWindow = 0.6f;
}