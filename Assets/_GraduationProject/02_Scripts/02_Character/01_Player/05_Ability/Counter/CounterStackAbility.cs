using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CounterStackAbility", menuName = "Project/Player/Ability/Ability/CounterStackAbility")]
public class CounterStackAbility : PlayerAbilitySO
{
    [SerializeField] private int _requiredCounterStacks = 3; // 필요한 카운터 스택 수
    private bool _checkCounterStack = false;

    /// <summary>
    /// 기능 등록
    /// </summary>
    /// <param name="ability">플레이어</param>
    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();
        p_tagInstances = new List<PlayerAbilityTagSO>();

        if(p_owner.Combat.CounterStacks >= _requiredCounterStacks)
        {
            _checkCounterStack = false;
            AddAllSkillTags();
        }
        else
        {
            _checkCounterStack = true;
        }

        p_owner.Combat.CounterStackChanged += OnCounterStackChanged;
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_owner.Combat.CounterStackChanged -= OnCounterStackChanged;

        base.UnregisterAbility(ability);
    }

    private void OnCounterStackChanged(int stack)
    {
        if (stack >= _requiredCounterStacks && _checkCounterStack)
        {
            _checkCounterStack = false;
            AddAllSkillTags();
        }
        else 
        {
            _checkCounterStack = true;
            RemoveAllSkillTags();
        }
    }
}
