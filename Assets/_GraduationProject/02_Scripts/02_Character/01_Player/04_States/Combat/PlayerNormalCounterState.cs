using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 플레이어의 일반 상쇄 상태입니다.
/// </summary>
public class PlayerNormalCounterState : PlayerAttackBaseState
{
    protected override IRuntimeAttackConfig p_AttackConfig => p_owner.Combat.NormalCounterAttackConfig;

    public PlayerNormalCounterState(StateMachine<PlayerController> stateMachine) 
        : base(stateMachine) { }

    #region Setup Function
    protected override void SetupEvents()
    {
        base.SetupEvents();

        p_owner.Events.CounterSucceeded += OnCounterSucceeded;
        p_owner.Combat.CheckedProjectileCounter += OnChecekdProjectileCounter;
    }

    protected override void SetupStats()
    {
        base.SetupStats();

    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        // 애니메이션 설정
        if(p_stateMachine.PreviousState.GetType() != typeof(PlayerChargeState))
        {
            p_animator.SetTrigger("Counter");
        }

        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.NormalCounterAttack);
    }
    #endregion

    #region Clear Function
    protected override void ClearEvents()
    {
        base.ClearEvents();

        p_owner.Events.CounterSucceeded -= OnCounterSucceeded;
        p_owner.Combat.CheckedProjectileCounter -= OnChecekdProjectileCounter;
    }

    protected override void ClearStats()
    {
        base.ClearStats();

        p_owner.Events.TriggerCounterWindowFinished();
        p_owner.Combat.ClearCounterEnemySet();
        p_owner.Combat.ClearCounterDamagedEnemy();

        // 상쇄로 인한 수퍼아머 태그가 있으면
        if (p_owner.Ability.HasTag(p_owner.Combat.CounterSuccessTagSO))
        {
            p_owner.Ability.RemoveTag(p_owner.Combat.CounterSuccessTagSO);
        }
    }

    #endregion

    #region Input

    /// <summary>
    /// 일반 상쇄 입력 처리
    /// </summary>
    protected override void OnNormalCounter()
    {
        return;
    }
    #endregion

    #region EventHandle
    protected override void OnAttackPerformed()
    {
        p_isAttackPerformed = true; // 현재 애니메이션의 시작 이벤트 확인
        Collider[] colldiers = p_owner.Combat.ExecuteAttack(p_AttackConfig);

        foreach (var collider in colldiers)
        {
            if (collider.TryGetComponent<IDamageable>(out var damageable) && !p_owner.Combat.IsEnemyCounterDamaged(damageable))
            {
                p_owner.Combat.AddCounterDamagedEnemy(damageable);
            }
        }
    }

    /// <summary>
    /// 상쇄 성공
    /// </summary>
    /// <param name="transform">상쇄한 적</param>
    private void OnCounterSucceeded(Transform transform, AttackType type)
    {
        // 상쇄 성공 시 슈퍼 아머
        if (!p_owner.Ability.HasTag(p_owner.Combat.CounterSuccessTagSO))
        {
            p_owner.Ability.AddTag(p_owner.Combat.CounterSuccessTagSO);
        }

        // 적이 상쇄되지 않았다면 상쇄
        if (transform.TryGetComponent<IParryable>(out var parryable) && !p_owner.Combat.IsEnemyCountered(parryable))
        {
            parryable.Parry(AttackType.Normal_Counter);
            p_owner.Combat.AddCounterEnemy(parryable);
        }

        // 적이 아직 죽지 않았다면 타격
        if (transform.TryGetComponent<IDamageable>(out var damageable))
        {
            float previousDamage = 0f;
            StatModifier prevDamageModifier = null;

            if (p_owner.Combat.IsEnemyCounterDamaged(damageable))
            {
                previousDamage = p_AttackConfig.Damage.Value;
                Debug.Log("삭제할 데미지: " + previousDamage);

                prevDamageModifier = new StatModifier(-previousDamage, StatModifierType.Flat, "prevDamage");
                p_AttackConfig.Damage.AddModifier(prevDamageModifier);
            }

            StatModifier NormalCounterModifier = new StatModifier(p_owner.Data.CounterDamageMultiply[0], StatModifierType.PercentAdd, "NormalCounter");
            p_AttackConfig.Damage.AddModifier(NormalCounterModifier);
            
            int finalDamage = (int)p_AttackConfig.Damage.Value;
            Debug.Log("상쇄 데미지: " + finalDamage);
            DamageData damage = new DamageData
            { 
                AttackerTransform = transform,
                AttackType = AttackType.Normal_Counter,
                DamageAmount = finalDamage,
                StiffnessAmount = 0,
                KnockbackCurve = p_AttackConfig.KnockbackConfig.StepCurve,
                KnockbackDuration = p_AttackConfig.KnockbackConfig.StepDuration,
                KnockbackForce = p_AttackConfig.KnockbackConfig.StepDistance,
            };

            int regainAmount = Mathf.RoundToInt(finalDamage * p_AttackConfig.Regain.Value);
            p_owner.Events.TriggerAttackRegained(regainAmount);

            p_owner.Combat.Attack(damageable, damage);

            p_AttackConfig.Damage.RemoveModifier(NormalCounterModifier);

            if (prevDamageModifier != null)
            {
                p_AttackConfig.Damage.RemoveModifier(prevDamageModifier);
            }
        }
    }

    private void OnChecekdProjectileCounter()
    {
        Vector3 attackCenter = p_owner.Combat.GetAttackCenter(p_AttackConfig);
        Vector3 halfExtents = p_AttackConfig.AttackRadius / 2f;

        Collider[] hitObjects = Physics.OverlapBox(attackCenter, halfExtents, p_owner.transform.rotation, p_owner.Data.AttackLayerMask);

        if (hitObjects.Length > 0)
        {
            foreach(Collider collider in hitObjects)
            {
                // 투사체인 경우
                if (collider.TryGetComponent<EnemyProjectile>(out var projectile))
                {
                    Vector3 direction = projectile.Owner.transform.position - p_owner.transform.position;
                    direction.Normalize();

                    DamageData damageData = projectile.Data;
                    damageData.DamageAmount += (int)p_AttackConfig.Damage.Value;
                    
                    float speed = projectile.MoveSpeed + p_owner.Combat.ProjectileCounterAddedVelocity[0];

                    projectile.Setup(direction, speed, p_owner.gameObject, damageData);

                    // 카운터 성공 이벤트 발행
                    p_owner.Events.TriggerCounterSucceeded(damageData.AttackerTransform, p_AttackConfig.AttackType);
                }
            }
        }
    }

    #endregion
}