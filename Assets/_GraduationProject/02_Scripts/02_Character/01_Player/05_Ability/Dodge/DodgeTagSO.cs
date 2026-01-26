using UnityEngine;

/// <summary>
/// 회피 테그 스크립터블 오브젝트
/// </summary>
[CreateAssetMenu(fileName = "DodgeTagSO", menuName = "Project/Player/Ability/Tag/Dodge/DodgeTagSO")]
public class DodgeTagSO : PlayerAbilityTagSO
{
    public int DodgeType = 0;
    public string AnimationStateName;   // 애니메이션 상태 이름
    public StepData DodgeData;          // 회피 데이터
    public float DamageReduction = 0;   // 데미지 감소량
}
