using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChangeCounterDamageMultiply", menuName = "Project/Player/Ability/Tag/ChangeCounterDamageMultiply")]
public class ChangeCounterDamageMultiply : PlayerAbilityTagSO
{
    public List<StatModifierConfig> ModifierConfigs;

    public override void Apply(PlayerController player)
    {
        for(int i = 0; i < ModifierConfigs.Count; i++)
        {
            StatModifier modifier = new StatModifier(ModifierConfigs[i], this);
            player.RuntimeData.CounterStackDamageMultipliers[i].AddModifier(modifier);
        }
    }

    public override void Revert(PlayerController player)
    {
        for(int i = 0; i < ModifierConfigs.Count; i++)
        {
            player.RuntimeData.CounterStackDamageMultipliers[i].RemoveAllModifiersFromSource(this);
        }
    }
}
