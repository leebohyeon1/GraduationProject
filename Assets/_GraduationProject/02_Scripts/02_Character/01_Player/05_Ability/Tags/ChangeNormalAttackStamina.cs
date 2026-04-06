using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ChangeNormalAttackStamina", menuName = "Project/Player/Ability/Tag/ChangeNormalAttackStamina")]
public class ChangeNormalAttackStamina : PlayerAbilityTagSO
{
    [SerializeField] private StatModifierConfig _modifier;

    public override void Apply(PlayerController player)
    {
        int normalAttackCount = player.RuntimeData.NormalAttacks.Count;
        for (int i = 0; i < normalAttackCount; i++)
        {
            player.RuntimeData.NormalAttacks[i].Stamina.AddModifier(new StatModifier(_modifier, this));
        }
    }

    public override void Revert(PlayerController player)
    {
        int normalAttackCount = player.RuntimeData.NormalAttacks.Count;
        for (int i = 0; i < normalAttackCount; i++)
        {
            player.RuntimeData.NormalAttacks[i].Stamina.RemoveAllModifiersFromSource(this);
        }
    }
}
