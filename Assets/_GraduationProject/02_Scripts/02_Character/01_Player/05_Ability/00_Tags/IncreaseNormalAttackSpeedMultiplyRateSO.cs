using UnityEngine;

/// <summary>
/// 일반 공격 공속 배율 증가
/// </summary>
[CreateAssetMenu(fileName = "IncreaseNormalAttackSpeedMultiplyRateSO", menuName = "Project/Player/Ability/Tag/[CreateAssetMenu(fileName = \"IncreaseNormalAttackSpeedMultiplyRateSO\", menuName = \"Project/Player/Ability/Tag/IncreaseNormalAttackSpeedRateSO\")]\r\n")]
public class IncreaseNormalAttackSpeedMultiplyRateSO : PlayerAbilityTagSO
{
    [Range(0, 1)]
    public float IncreaseRate;    // 증가량

    public override void Apply(PlayerController player)
    {
        player.Combat.IncreaseNormalAttackSpeedMultiplyRate(IncreaseRate);
    }

    public override void Revert(PlayerController player)
    {
        player.Combat.DecreaseNormalAttackSpeedMultiplyRate(IncreaseRate);
    }
}
