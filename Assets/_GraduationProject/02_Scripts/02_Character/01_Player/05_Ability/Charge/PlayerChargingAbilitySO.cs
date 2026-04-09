using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 차징 중에 특정 태그를 부여하고 차징이 끝나면 제거하는 능력
/// </summary>
[CreateAssetMenu(fileName = "PlayerChargingAbilitySO", menuName = "Project/Player/Ability/Ability/ChargingAbility")]
public class PlayerChargingAbilitySO : PlayerAbilitySO
{
    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();
        p_tagInstances = new List<PlayerAbilityTagSO>();

        // 플레이어의 차징 이벤트 구독 (이벤트 이름은 프로젝트의 PlayerController 구조에 따라 다를 수 있습니다)
        if (p_owner != null)
        {
            p_owner.Events.ChargeStarted += HandleChargeStart;
            p_owner.Events.ChargeCompleted += HandleChargeEnd;
        }
    }


    public override void UnregisterAbility(PlayerAbility ability)
    {
        if (p_owner != null)
        {
            p_owner.Events.ChargeStarted -= HandleChargeStart;
            p_owner.Events.ChargeCompleted -= HandleChargeEnd;
        }

        base.UnregisterAbility(ability);
    }

    private void HandleChargeStart()
    {
        AddAllSkillTags();
    }

    private void HandleChargeEnd(bool obj)
    {
        RemoveAllSkillTags();
    }
}
