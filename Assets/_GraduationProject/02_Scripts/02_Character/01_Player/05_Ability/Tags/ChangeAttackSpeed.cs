using UnityEngine;

[CreateAssetMenu(fileName = "ChangeAttackSpeed", menuName = "Project/Player/Ability/Tag/ChangeAttackSpeed")]
public class ChangeAttackSpeed : PlayerAbilityTagSO
{
    [SerializeField] private StatModifierConfig _modifier;

    public override void Apply(PlayerController player)
    {
        player.RuntimeData.AttackSpeed.AddModifier(new StatModifier(_modifier, this));
    }

    public override void Revert(PlayerController player)
    {
        player.RuntimeData.AttackSpeed.RemoveAllModifiersFromSource(this);
    }
}
