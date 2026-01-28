using UnityEngine;

/// <summary>
/// 일반 공격 공속 배율 증가
/// </summary>
[CreateAssetMenu(fileName = "IncreaseAttackDamageMultiplyRateSO", menuName = "Project/Player/Ability/Tag/IncreaseAttackDamageMultiplyRateSO")]
public class IncreaseAttackDamageMultiplyRateSO : PlayerAbilityTagSO
{
    [Range(0, 1)]
    public float IncreaseMultiplier;    // 증가량

    public override void Apply(PlayerController player)
    {
        player.Combat.IncreaseAttackDamageMultiplier(IncreaseMultiplier);
    }

    public override void Revert(PlayerController player)
    {
        player.Combat.DecreaseAttackDamageMultiplier(IncreaseMultiplier);
    }
}
