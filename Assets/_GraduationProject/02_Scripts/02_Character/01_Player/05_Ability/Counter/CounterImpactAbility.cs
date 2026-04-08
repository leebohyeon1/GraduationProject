using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CounterImpactAbility", menuName = "Project/Player/Ability/Ability/CounterImpactAbility")]
public class CounterImpactAbility : PlayerAbilitySO
{
    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();
        p_tagInstances = new List<PlayerAbilityTagSO>();

        p_owner.Events.CounterSucceeded += OnCounterSucceeded;
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_owner.Events.CounterSucceeded -= OnCounterSucceeded;

        base.UnregisterAbility(ability);
    }

    protected virtual void OnCounterSucceeded(Transform transform, AttackType type)
    {
        if (type == AttackType.Normal_Counter || type == AttackType.Strong_Counter)
        {
            // 카운터 성공 시 적용할 효과 추가
            // 예: 체력 회복, 버프 적용 등
            AddAllSkillTags();
            RemoveAllSkillTags();
        }
    }
}
