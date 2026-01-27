using UnityEngine;

/// <summary>
/// 보호막 획득 태그
/// </summary>
[CreateAssetMenu(fileName = "GainShieldSO", menuName = "Project/Player/Ability/Tag/GainShieldSO")]
public class GainShieldSO : PlayerAbilityTagSO
{
    public int ShieldAmount;    // 획득할 보호막 양

    public override void Apply(PlayerController player)
    {
        base.Apply(player);

        player.Health.IncreaseDamageReduction(ShieldAmount);
    }
}
