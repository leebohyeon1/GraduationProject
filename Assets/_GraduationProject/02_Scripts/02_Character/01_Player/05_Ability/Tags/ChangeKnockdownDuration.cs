using UnityEngine;

[CreateAssetMenu(fileName = "ChangeKnockdownDuration", menuName = "Project/Player/Ability/Tag/ChangeKnockdownDuration")]
public class ChangeKnockdownDuration : PlayerAbilityTagSO
{
    public StatModifierConfig ModifierConfig;

    public override void Apply(PlayerController player)
    {
        StatModifier modifier = new StatModifier(ModifierConfig, this);
        player.RuntimeData.KnockDownDuration.AddModifier(modifier);
    }


    public override void Revert(PlayerController player)
    {
        player.RuntimeData.KnockDownDuration.RemoveAllModifiersFromSource(this);
    }
}
