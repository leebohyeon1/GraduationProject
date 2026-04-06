using Unity.XR.OpenVR;
using UnityEngine;

[CreateAssetMenu(fileName = "CanRegainSO", menuName = "Project/Player/Ability/Tag/CanRegainSO")]
public class CanRegainSO : PlayerAbilityTagSO
{
    [Range(0, 10)]
    public float RegainRate;    // 회복 비율

    public override void Apply(PlayerController player)
    {
        player.Combat.IncreaseAttackRegainRate(RegainRate);
    }

    public override void Revert(PlayerController player)
    {
        player.Combat.DecreaseAttackRegainRate(RegainRate);
    }

}
