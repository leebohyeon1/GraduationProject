using UnityEngine;

[CreateAssetMenu(fileName = "ChangeStamina", menuName = "Project/Player/Ability/Tag/ChangeStamina")]
public class ChangeStamina : PlayerAbilityTagSO
{
    [SerializeField] private float staminaChangeAmount;

    public override void Apply(PlayerController player)
    {
        player.Stamina.ChangeStamina(staminaChangeAmount);
    }

    public override void Revert(PlayerController player)
    {
        base.Revert(player);
    }
}
