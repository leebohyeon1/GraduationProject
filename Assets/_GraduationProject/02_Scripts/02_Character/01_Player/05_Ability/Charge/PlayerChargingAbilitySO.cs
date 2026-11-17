using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerChargingAbilitySO", menuName = "Project/Player/Ability/Ability/ChargingAbility")]
public class PlayerChargingAbilitySO : PlayerAbilitySO
{
    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = ability.GetComponent<PlayerController>();
        p_tagInstances = new List<PlayerAbilityTagSO>();
        p_owner.FSM.OnStateChanged += HandleStateChanged;
        HandleStateChanged(p_owner.FSM.CurrentState);
    }
    public override void UnregisterAbility(PlayerAbility ability)
    {
        if (p_owner && p_owner.FSM != null) p_owner.FSM.OnStateChanged -= HandleStateChanged;
        base.UnregisterAbility(ability);
    }
    private void HandleStateChanged(IState state)
    {
        // Cover the initial hold/spin, charging and the released charged parry.
        bool charging = state is PlayerChargeState || state is PlayerHeavyCounterState;
        if (charging && p_tagInstances.Count == 0) AddAllSkillTags();
        else if (!charging && p_tagInstances.Count > 0) RemoveAllSkillTags();
    }
}
