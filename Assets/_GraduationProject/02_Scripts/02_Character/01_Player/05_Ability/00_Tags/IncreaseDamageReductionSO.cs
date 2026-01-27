using UnityEngine;

/// <summary>
/// 피해 감소량 증가 태그
/// </summary>
[CreateAssetMenu(fileName = "IncreaseDamageReductionSO", menuName = "Project/Player/Ability/Tag/IncreaseDamageReductionSO")]
public class IncreaseDamageReductionSO : PlayerAbilityTagSO
{
    public float IncreaseAmount;    // 증가량

    public override void Apply(PlayerController player)
    {
        player.Health.IncreaseDamageReduction(IncreaseAmount);
    }

    public override void Revert(PlayerController player)
    {
        player.Health.DecreaseDamageReduction(IncreaseAmount);
    }
}
