using UnityEngine;

[CreateAssetMenu(fileName = "IgnoreStrongAttackStack", menuName = "Project/Player/Ability/Tag/IgnoreStrongAttackStack")]
public class IgnoreStrongAttackStack : PlayerAbilityTagSO
{
    private PlayerController _player;

    public override void Apply(PlayerController player)
    {
        base.Apply(player);
    }

    public override void Revert(PlayerController player)
    {
        base.Revert(player);
    }
}
