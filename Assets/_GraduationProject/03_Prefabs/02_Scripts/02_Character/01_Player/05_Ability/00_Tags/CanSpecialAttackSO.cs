using UnityEngine;

/// <summary>
/// 특별 공격 태그
/// </summary>
[CreateAssetMenu(fileName = "CanSpecialAttackSO", menuName = "Project/Player/Ability/Tag/CanSpecialAttackSO")]
public class CanSpecialAttackSO : PlayerAbilityTagSO
{
    public string AnimationTigger;
    public PlayerAttackConfig AttackConfig;  //  기본 데미지

    public override void Apply(PlayerController player)
    {
        base.Apply(player);

        player.Combat.SetSpecialAttackSO(this);

        // 특수 공격 상태로 전환
        player.FSM.ChangeState<PlayerSpecialAttackState>();
    }

    public override void Revert(PlayerController player)
    {
        base.Revert(player);

        player.Combat.ClearSpecialAttackSO();
    }
}
