using UnityEngine;

/// <summary>
/// 플레이어의 차지 공격 상태입니다.
/// </summary>
public class PlayerHeavyCounterState : PlayerAttackBaseState
{
    protected override IRuntimeAttackConfig p_AttackConfig => _ChargeAttackConfig;

    private RuntimeChargeAttackConfig _ChargeAttackConfig => p_owner.Combat.HeavyCounterAttackConfig;

    public PlayerHeavyCounterState(StateMachine<PlayerController> stateMachine)
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

        p_owner.Combat.ResetNormalAttackComboIndex();       // 일반 공격 콤보 순서 초기화
        p_owner.Combat.ResetHeavyAttackComboIndex();       // 강공격 콤보 순서 초기화
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        // 애니메이션 설정
        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.ChargeCounterAttack);
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
        p_owner.Combat.SetCharge(false);
        p_owner.Events.TriggerChargeCompleted(false);

        // 상쇄로 인한 수퍼아머 태그가 있으면
        if (p_owner.Ability.HasTag(p_owner.Combat.CounterSuccessTagSO))
        {
            p_owner.Ability.RemoveTag(p_owner.Combat.CounterSuccessTagSO);
        }
    }

    #endregion

    #region EventHandle
    protected override void OnAttackPerformed()
    {
        p_isAttackPerformed = true; // 현재 애니메이션의 시작 이벤트 확인
        Collider[] colldiers = p_owner.Combat.ExecuteAttack(p_AttackConfig);

        foreach (var collider in colldiers)
        {
            if(collider.TryGetComponent<IDamageable>(out var damageable) && !p_owner.Combat.IsEnemyCounterDamaged(damageable))
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
            parryable.Parry(AttackType.Strong_Counter);
            p_owner.Combat.AddCounterEnemy(parryable);
            p_owner.Events.TriggerOnlyChargeAttackSucceded();
        }

        // 적이 아직 죽지 않았다면 타격
        if (transform.TryGetComponent<IDamageable>(out var damageable))
        {
            // 1. 현재 데미지(기본)를 미리 저장
            int baseDamage = (int)p_AttackConfig.Damage.Value;

            // 2. 카운터 배율 적용
            StatModifier NormalCounterModifier = new StatModifier(p_owner.Data.CounterDamageMultiply[1], StatModifierType.PercentAdd, "HeavyCounter");
            p_AttackConfig.Damage.AddModifier(NormalCounterModifier);

            // 3. 전체 카운터 데미지 계산
            int totalCounterDamage = (int)p_AttackConfig.Damage.Value;
            int finalDamage = totalCounterDamage;

            // 4. 이미 데미지를 입었다면 전체에서 기본값만큼을 뺌 (추가 데미지만 남김)
            if (p_owner.Combat.IsEnemyCounterDamaged(damageable))
            {
                finalDamage -= baseDamage;
            }

            finalDamage = Mathf.Max(0, finalDamage);
            Debug.Log("최종 데미지: " + finalDamage);

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
        }
    }

    private void OnChecekdProjectileCounter()
    {
        Vector3 attackCenter = p_owner.Combat.GetAttackCenter(_ChargeAttackConfig.BaseAttackConfig);
        Vector3 halfExtents = _ChargeAttackConfig.AttackRadius / 2f;

        Collider[] hitObjects = Physics.OverlapBox(attackCenter, halfExtents, p_owner.transform.rotation, p_owner.Data.AttackLayerMask);

        if (hitObjects.Length > 0)
        {
            foreach (Collider collider in hitObjects)
            {
                // 투사체인 경우
                if (collider.TryGetComponent<EnemyProjectile>(out var projectile))
                {
                    Vector3 direction = projectile.Owner.transform.position - p_owner.transform.position;
                    direction.Normalize();

                    DamageData damageData = projectile.Data;
                    damageData.DamageAmount += (int)_ChargeAttackConfig.Damage.Value;

                    float speed = projectile.MoveSpeed + p_owner.Combat.ProjectileCounterAddedVelocity[p_owner.Combat.IsCharge ? 1 : 0];

                    projectile.Setup(projectile._enemy,direction, speed, p_owner.gameObject, damageData);

                    // 투사체를 튕겨낼 시 
                    // 상쇄 이벤트 발생
                    p_owner.Events.TriggerCounterSucceeded(projectile.transform, p_AttackConfig.AttackType); 
                    p_owner.Events.TriggerOnlyChargeAttackSucceded();
                }
            }
        }
    }
    #endregion
}
