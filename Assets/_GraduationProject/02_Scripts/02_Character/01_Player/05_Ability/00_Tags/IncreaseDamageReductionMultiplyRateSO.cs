using UnityEngine;

/// <summary>
/// 피해 감소량 증가 태그
/// </summary>
[CreateAssetMenu(fileName = "IncreaseDamageReductionSO", menuName = "Project/Player/Ability/Tag/IncreaseDamageReductionSO")]
public class IncreaseDamageReductionMultiplyRateSO : PlayerAbilityTagSO
{
    [Range(0, 1)]
    public float IncreaseRate;    // 증가량

    public override void Apply(PlayerController player)
    {
        player.Health.IncreaseDamageReductionMultiplyRate(IncreaseRate);
    }

    public override void Revert(PlayerController player)
    {
        player.Health.DecreaseDamageReductionMultiplyRate(IncreaseRate);
    }
}
