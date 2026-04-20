using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 패링 성공 시 적에게 '낙인'을 찍어 일정 시간 동안 모든 공격에 추가 데미지를 입히는 어빌리티입니다.
/// </summary>
[CreateAssetMenu(fileName = "StigmaCrasher", menuName = "Project/Player/Ability/Ability/StigmaCrasher")]
public class StigmaCrasher : CounterImpactAbility
{
    [Header("Stigma Settings")]
    [SerializeField] private float _stigmaCrasherDuration = 5f; // 낙인 지속 시간
    [SerializeField] private StatModifierConfig _modifier;   // 데미지 배율 정보가 담긴 태그

    private HashSet<IDamageable> _affectedTargets = new HashSet<IDamageable>();
    private bool _shouldRemoveModifier = false;
    private Stat _currentStat;

    public override void RegisterAbility(PlayerAbility ability)
    {
        base.RegisterAbility(ability);
        
        if (p_owner != null)
        {
            p_owner.Events.BeforeDamageCalculate += OnBeforeDamageCalculate;
            p_owner.Combat.AttackEvent += OnAttackEvent;
        }
    }


    public override void UnregisterAbility(PlayerAbility ability)
    {
        if (p_owner != null)
        {
            p_owner.Events.BeforeDamageCalculate -= OnBeforeDamageCalculate;
            p_owner.Combat.AttackEvent -= OnAttackEvent;
        }

        base.UnregisterAbility(ability);
        _affectedTargets.Clear();
    }

    /// <summary>
    /// 공격 적중 직전에 호출되어 특정 적에게만 스탯 모디파이어를 적용합니다.
    /// </summary>
    private void OnBeforeDamageCalculate(Transform target, Stat damageStat)
    {
        if(target.TryGetComponent<IDamageable>(out var damageable))
        {
            // 타겟이 낙인 대상이고, 능력치 설정 및 스탯 객체가 유효하다면
            if (_affectedTargets.Contains(damageable) && damageStat != null)
            {
                _currentStat = damageStat;

                var modifier = new StatModifier(_modifier, this);
                _currentStat.AddModifier(modifier);

                _shouldRemoveModifier = true;
            }
        }
    }

    private void OnAttackEvent(IDamageable damageable, DamageData data)
    {
        if(!_shouldRemoveModifier && _currentStat != null)
        {
            return;
        }

        _currentStat.RemoveAllModifiersFromSource(this);
        _currentStat = null;
        _shouldRemoveModifier = false;
    }

    protected override void OnCounterSucceeded(Transform transform, AttackType type)
    {
        if (transform != null && p_owner != null)
        {
            p_owner.StartCoroutine(StigmaCrasherDurationCoroutine(transform));
        }
    }

    private IEnumerator StigmaCrasherDurationCoroutine(Transform target)
    {
        if (target == null) yield break;

        if (target.TryGetComponent<IDamageable>(out var damageable))
        {
            _affectedTargets.Add(damageable);

            yield return new WaitForSeconds(_stigmaCrasherDuration);

            if (target != null)
            {
                _affectedTargets.Remove(damageable);
            }

            _affectedTargets.RemoveWhere(t => t == null);
        }
        
    }
}
