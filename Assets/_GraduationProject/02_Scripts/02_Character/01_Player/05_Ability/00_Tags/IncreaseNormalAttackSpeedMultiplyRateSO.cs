using UnityEngine;

/// <summary>
/// 일반 공격 공속 배율 증가
/// </summary>
[CreateAssetMenu(fileName = "IncreaseNormalAttackSpeedMultiplyRateSO", menuName = "Project/Player/Ability/Tag/IncreaseNormalAttackSpeedMultiplyRateSO")]
public class IncreaseNormalAttackSpeedMultiplyRateSO : PlayerAbilityTagSO
{
    [Range(0, 10)]
    public float IncreaseMultiplier;    // 증가량

    public override void Apply(PlayerController player)
    {
        player.Combat.IncreaseNormalAttackSpeedMultiplier(IncreaseMultiplier);
    }

    public override void Revert(PlayerController player)
    {
        player.Combat.DecreaseNormalAttackSpeedMultiplier(IncreaseMultiplier);
    }
}
