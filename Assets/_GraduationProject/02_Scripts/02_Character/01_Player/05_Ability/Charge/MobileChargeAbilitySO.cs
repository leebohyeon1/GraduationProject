using UnityEngine;

/// <summary>
/// 플레이어의 기동차징 어빌리티입니다.
/// 차징 중에 이동과 대시를 가능하게 합니다.
/// </summary>
[CreateAssetMenu(fileName = "MobileChargeAbilitySO", menuName = "Project/Player/Ability/Ability/MobileChargeAbility")]
public class MobileChargeAbilitySO : PlayerAbilitySO
{
    [Header("Charge Dash Setting")]
    public string DashAnimationName = "ChargeDash"; // 차지 대시 애니메이션 이름
    public float DashDistance = 3f;                // 대시 거리
    public float DashDuration = 0.2f;              // 대시 소요 시간
    public AnimationCurve DashCurve = AnimationCurve.Linear(0, 0, 1, 1); // 대시 속도 커브
}
