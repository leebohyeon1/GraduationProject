using UnityEngine;

/// <summary>
/// 보호막 획득 태그
/// </summary>
[CreateAssetMenu(fileName = "GainMaxHealthShieldSO", menuName = "Project/Player/Ability/Tag/GainMaxHealthShieldSO")]
public class GainMaxHealthShieldSO : PlayerAbilityTagSO
{
    [Range(0, 10)]
    public float shieldPercentOfMaxHealth;

    public override void Apply(PlayerController player)
    {
        base.Apply(player);

        // 실드량 계산
        float shieldAmount = player.Health.MaxHealth * shieldPercentOfMaxHealth;
        // 반올림
        player.Health.IncreaseDamageReduction(Mathf.RoundToInt(shieldAmount));
    }
}
