using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SeamlessFlow", menuName = "Project/Player/Ability/Ability/SeamlessFlow")]
public class SeamlessFlow : PlayerAbilitySO
{
    [SerializeField] private int _requiredCounterStacks = 3; // 필요한 카운터 스택 수
    [SerializeField] private float _duration = 5.0f;          // 태그 유지 시간
    private bool _isActive = false; // 기능 활성화 여부    
    private Coroutine _timerCoroutine;

    /// <summary>
    /// 기능 등록
    /// </summary>
    /// <param name="ability">플레이어</param>
    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();
        p_tagInstances = new List<PlayerAbilityTagSO>();


        p_owner.Events.CounterSucceeded += OnCounterSucceeded;
        
        // 제거 조건들 등록
        p_owner.Events.DodgeStarted += RemoveAllSkillTags;
        p_owner.Events.AttackStarted += RemoveAllSkillTags;
        p_owner.Events.Damaged += OnDamaged;
        p_owner.Events.Knockdown += RemoveAllSkillTags;
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_owner.Events.CounterSucceeded -= OnCounterSucceeded;
        
        p_owner.Events.DodgeStarted -= RemoveAllSkillTags;
        p_owner.Events.AttackStarted -= RemoveAllSkillTags;
        p_owner.Events.Damaged -= OnDamaged;
        p_owner.Events.Knockdown -= RemoveAllSkillTags;

        StopTimer();
        base.UnregisterAbility(ability);
    }

    private void OnCounterSucceeded(Transform transform, AttackType type)
    {
        if(type == AttackType.Normal_Counter || type == AttackType.Strong_Counter)
        {
            if (_timerCoroutine != null)
            {
                return;
            }

            if (p_owner.Combat.CounterStacks >= _requiredCounterStacks)
            {
                AddAllSkillTags();
                StartTimer();
            }
        }
    }

    private void OnDamaged(DamageData data)
    {
        RemoveAllSkillTags();
    }

    protected override void RemoveAllSkillTags()
    {
        base.RemoveAllSkillTags();
        StopTimer();
    }

    private void StartTimer()
    {
        StopTimer();
        _timerCoroutine = p_owner.StartCoroutine(TimerRoutine());
    }

    private void StopTimer()
    {
        if (_timerCoroutine != null)
        {
            p_owner.StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
    }

    private System.Collections.IEnumerator TimerRoutine()
    {
        yield return new WaitForSeconds(_duration);
        RemoveAllSkillTags();
    }
}
