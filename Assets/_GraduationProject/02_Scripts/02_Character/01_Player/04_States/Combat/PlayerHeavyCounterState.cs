using UnityEngine;

/// <summary>
/// 플레이어의 차지 공격 상태입니다.
/// </summary>
public class PlayerHeavyCounterState : PlayerAttackBaseState
{
    protected override PlayerAttackConfig p_AttackConfig => p_owner.Combat.HeavyCounterAttackConfig.AttackConfig;

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
        p_owner.Combat.SetCharge(false);
        p_owner.Events.TriggerChargeCompleted(false);

        // 상쇄로 인한 수퍼아머 태그가 있으면
        if (p_owner.Ability.HasTag(p_owner.Combat.CounterSuccessTagSO))
        {
            p_owner.Ability.RemoveTag(p_owner.Combat.CounterSuccessTagSO);
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
        if (!p_owner.Ability.HasTag(p_owner.Combat.CounterSuccessTagSO))
        {
            p_owner.Ability.AddTag(p_owner.Combat.CounterSuccessTagSO);
        }

        // 적이 상쇄되지 않았다면 상쇄
        if (transform.TryGetComponent<IParryable>(out var parryable) && !p_owner.Combat.IsEnemyCountered(parryable))
        {
            parryable.Parry(AttackType.HeavyCounter);
            p_owner.Combat.AddCounterEnemy(parryable);
            p_owner.Events.TriggerOnlyChargeAttackSucceded();
        }


        // 적이 아직 죽지 않았다면 타격
        if (transform.TryGetComponent<IDamageable>(out var damageable))
        {
            DamageData damage = new DamageData
            {
                AttackerTransform = transform,
                AttackType = AttackType.HeavyCounter,
                DamageAmount = p_owner.Combat.CalculateFinalDamage(p_AttackConfig.AttackDamage, 1),
                StiffnessAmount = 0,
                KnockbackCurve = p_AttackConfig.KnockbackCofig.StepCurve,
                KnockbackDuration = p_AttackConfig.KnockbackCofig.StepDuration,
                KnockbackForce = p_AttackConfig.KnockbackCofig.StepDistance,
            };

            Debug.Log("강패링 데미지: " + damage.DamageAmount);
            p_owner.Combat.Attack(damageable, damage);
        }
    }

    /// <summary>
    /// 공격 판정이 발생하는 시점에 호출됩니다.
    /// </summary>
    protected override void OnAttackPerformed()
    {
        p_isAttackActive = true;

        Collider[] colliders = p_owner.Combat.ExecuteAttack(p_AttackConfig);
    }

    private void OnChecekdProjectileCounter()
    {
        Vector3 attackCenter = p_owner.Combat.GetAttackCenter(p_AttackConfig);
        Vector3 halfExtents = p_AttackConfig.AttackRadius / 2f;

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
                    damageData.DamageAmount += p_AttackConfig.AttackDamage;

                    float speed = projectile.MoveSpeed + p_owner.Combat.ProjectileCounterAddedVelocity[p_owner.Combat.IsCharge ? 1 : 0];

                    projectile.Setup(direction, speed, p_owner.gameObject, damageData);

                    // 투사체를 튕겨낼 시 
                    // 상쇄 이벤트 발생
                    p_owner.Events.TriggerCounterSucceeded(projectile.transform); 
                    p_owner.Events.TriggerOnlyChargeAttackSucceded();
                }
            }
        }
    }
    #endregion
}
