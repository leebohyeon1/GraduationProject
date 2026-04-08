using UnityEngine;

[CreateAssetMenu(fileName = "ChangeCounterStackDuration", menuName = "Project/Player/Ability/Tag/ChangeCounterStackDuration")]
public class ChangeCounterStackDuration : PlayerAbilityTagSO
{
    public StatModifierConfig ModifierConfig;

    public override void Apply(PlayerController player)
    {
        StatModifier modifier = new StatModifier(ModifierConfig, this);
        player.RuntimeData.CounterStackDuration.AddModifier(modifier);
    }


    public override void Revert(PlayerController player)
    {
        player.RuntimeData.CounterStackDuration.RemoveAllModifiersFromSource(this);
    }
}
