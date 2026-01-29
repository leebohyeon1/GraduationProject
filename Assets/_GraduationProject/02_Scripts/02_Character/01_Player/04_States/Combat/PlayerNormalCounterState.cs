using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 플레이어의 일반 상쇄 상태입니다.
/// </summary>
public class PlayerNormalCounterState : PlayerAttackBaseState
{
    protected override PlayerAttackConfig p_AttackConfig => p_owner.Combat.NormalCounterAttackConfig;

    public PlayerNormalCounterState(StateMachine<PlayerController> stateMachine) 
        : base(stateMachine) { }

    #region Setup Function
    protected override void SetupEvents()
    {
        base.SetupEvents();

        p_owner.Events.CounterSucceeded += OnCounterSucceeded;
    }

    protected override void SetupStats()
    {
        base.SetupStats();

    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        // 애니메이션 설정
        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.NormalCounterAttack);
    }
    #endregion

    #region Clear Function
    protected override void ClearEvents()
    {
        base.ClearEvents();

        p_owner.Events.CounterSucceeded -= OnCounterSucceeded;
    }

    protected override void ClearStats()
    {
        base.ClearStats();

        p_owner.Events.TriggerCounterWindowFinished();
        p_owner.Combat.ClearCounterEnemySet();

        // Smahs 가능 상태면 취소
        if (p_owner.Ability.HasTag("Smash_Attack"))
        {
            CanSpecialAttackSO smashAttackTag 
                = p_owner.Ability.GetTag("Smash_Attack") as CanSpecialAttackSO;

            p_owner.Ability.RemoveTag(smashAttackTag);
        }

        // 상쇄로 인한 수퍼아머 태그가 있으면
        if(p_owner.Ability.HasTag(p_owner.Combat.CounterSuperArmorTagSO))
        {
            p_owner.Ability.RemoveTag(p_owner.Combat.CounterSuperArmorTagSO);
        }
    }

    #endregion

    #region Input
    /// <summary>
    /// 공격 입력 처리
    /// </summary>
    protected override void OnNormalAttack()
    {
        // 일반 공격이 가능하지 않으면 리턴
        if (!p_owner.Combat.CanNormalAttack())
        {
            return;
        }

        // Smash가 가능하면 Smahs
        if (p_owner.Ability.HasTag("Smash_Attack"))
        {
            SmashSO smash = p_owner.Ability.GetAbility("Smash") as SmashSO;
            smash.Smash();
            return;
        }

        // 선입력 가능하면 공격 상태 변경
        if (p_nextState == null && p_canBufferInput)
        {
            p_stateMachine.ChangeState<PlayerNormalAttackState>();
        }
    }

    #endregion

    #region EventHandle
    /// <summary>
    /// 상쇄 성공
    /// </summary>
    /// <param name="transform">상쇄한 적</param>
    private void OnCounterSucceeded(Transform transform)
    {
        // 상쇄 성공 시 슈퍼 아머
        if (!p_owner.Ability.HasTag(p_owner.Combat.CounterSuperArmorTagSO))
        {
            p_owner.Ability.AddTag(p_owner.Combat.CounterSuperArmorTagSO);
        }

        // 적이 상쇄되지 않았다면 상쇄
        if (transform.TryGetComponent<IParryable>(out var parryable) && !p_owner.Combat.IsEnemyCountered(parryable))
        {
            parryable.Parry(AttackType.NormalCounter);
            p_owner.Combat.AddCounterEnemy(parryable);
        }

        // 적이 아직 죽지 않았다면 타격
        if (transform.TryGetComponent<IDamageable>(out var damageable))
        {
            DamageData damage = new DamageData
            { 
                AttackerTransform = transform,
                AttackType = p_AttackConfig.AttackType,
                DamageAmount = p_AttackConfig.AttackDamage,
                StiffnessAmount = 0,
                KnockbackCurve = p_AttackConfig.KnockbackCofig.StepCurve,
                KnockbackDuration = p_AttackConfig.KnockbackCofig.StepDuration,
                KnockbackForce = p_AttackConfig.KnockbackCofig.StepDistance,
            };

            p_owner.Combat.Attack(damageable, damage);
        }
    }

    /// <summary>
    /// 공격 판정이 발생하는 시점에 호출됩니다.
    /// </summary>
    protected override void OnAttackPerformed()
    {
        Collider[] colliders = p_owner.Combat.ExecuteAttack(p_AttackConfig);

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<IParryable>(out var parryable))
            {
                p_owner.Combat.AddCounterEnemy(parryable);
            }
        }
    }
    #endregion
}