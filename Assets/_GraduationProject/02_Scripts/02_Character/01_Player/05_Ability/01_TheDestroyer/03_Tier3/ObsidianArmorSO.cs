using UnityEngine;

/// <summary>
/// 중량형 3티어 
/// 상쇄 성공 시 보호막 획득
/// 차징 공격 적중 시 보호막 획득
/// </summary>
[CreateAssetMenu(fileName = "ObsidianArmorSO", menuName = "Project/Player/Ability/TheDestroyer/Tier3/ObsidianArmorSO")]
public class ObsidianArmorSO : PlayerAbilitySO
{
    public int CounterShieldGainAmount = 0;
    public int ChargeAttackShieldGainAmount = 0;

    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();

        p_owner.Events.CounterSucceeded += OnCounterSucceeded;
        p_owner.Events.OnlyChargeAttackSucceded += OnOnlyChargeAttackSucceded;
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_ability = null;
        p_owner = null;

        p_owner.Events.CounterSucceeded -= OnCounterSucceeded;
        p_owner.Events.OnlyChargeAttackSucceded -= OnOnlyChargeAttackSucceded;
    }

    private void OnCounterSucceeded(Transform transform)
    {
        p_owner.Health.IncreaseShield(CounterShieldGainAmount);
    }

    private void OnOnlyChargeAttackSucceded()
    {
        p_owner.Health.IncreaseShield(ChargeAttackShieldGainAmount);
    }
}
