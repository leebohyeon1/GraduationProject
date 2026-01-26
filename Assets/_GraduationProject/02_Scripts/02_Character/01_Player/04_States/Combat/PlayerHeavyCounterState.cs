using UnityEngine;

/// <summary>
/// 플레이어의 차지 공격 상태입니다.
/// </summary>
public class PlayerHeavyCounterState : PlayerAttackBaseState
{
    protected override PlayerAttackConfig p_AttackConfig => p_owner.Data.HeavyCounterAttackConfigList[p_owner.Combat.ChargeLevel].AttackConfig;
     
    public PlayerHeavyCounterState(StateMachine<PlayerController> stateMachine)
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


        p_owner.Combat.ResetNormalAttackComboIndex();       // 일반 공격 콤보 순서 초기화
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        // 애니메이션 설정
        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.ChargeCounterCounterAttack);
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

        p_owner.Combat.ClearCounterEnemySet();
        p_owner.Combat.ResetChargeLevel();
    }

    #endregion

    #region EventHandle
    /// <summary>
    /// 상쇄 성공
    /// </summary>
    /// <param name="transform">상쇄한 적</param>
    private void OnCounterSucceeded(Transform transform)
    {
        // 적이 상쇄되지 않았다면 상쇄
        if (transform.TryGetComponent<IParryable>(out var parryable) && !p_owner.Combat.IsEnemyCountered(parryable))
        {
            parryable.Parry(AttackType.HeavyCounter);
            p_owner.Combat.AddCounterEnemy(parryable);
        }

        // 적이 아직 죽지 않았다면 타격
        if (transform.TryGetComponent<IDamageable>(out var damageable) && !damageable.IsDead)
        {
            damageable.TakeDamage(new DamageData(transform, p_AttackConfig.AttackType, p_AttackConfig.AttackDamage
                , 0, p_AttackConfig.KnockbackCofig.StepCurve, p_AttackConfig.KnockbackCofig.StepDuration, p_AttackConfig.KnockbackCofig.StepDistance));
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
