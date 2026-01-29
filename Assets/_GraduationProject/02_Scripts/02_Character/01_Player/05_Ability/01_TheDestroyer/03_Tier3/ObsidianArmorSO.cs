using UnityEngine;

/// <summary>
/// 중량형 3티어 
/// 상쇄 성공 시 보호막 획득
/// 차징 공격 적중 시 보호막 획득
/// </summary>
[CreateAssetMenu(fileName = "ObsidianArmorSO", menuName = "Project/Player/Ability/TheDestroyer/Tier3/ObsidianArmorSO")]
public class ObsidianArmorSO : PlayerAbilitySO
{
    public GainMaxHealthShieldSO GainCounterShieldSO;
    public GainMaxHealthShieldSO GainChargeAttackShieldSO;

    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();


        p_owner.Events.CounterSucceeded += OnCounterSucceeded;
        p_ability.AddTag(GainCounterShieldSO);

        p_owner.Events.OnlyChargeAttackSucceded += OnOnlyChargeAttackSucceded;
        p_ability.AddTag(GainChargeAttackShieldSO);
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_owner.Events.CounterSucceeded -= OnCounterSucceeded;
        p_ability.RemoveTag(GainCounterShieldSO);

        p_owner.Events.OnlyChargeAttackSucceded -= OnOnlyChargeAttackSucceded;
        p_ability.RemoveTag(GainChargeAttackShieldSO);

        p_ability = null;
        p_owner = null;
    }

    private void OnCounterSucceeded(Transform transform)
    {
        GainCounterShieldSO.Apply(p_owner);

        // 경직도 초기화
        p_owner.Health.ResetStiffness();
    }

    private void OnOnlyChargeAttackSucceded()
    {
        GainChargeAttackShieldSO.Apply(p_owner);
    }
}
