using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 플레이어의 일반 상쇄 상태입니다.
/// </summary>
public class PlayerParryState : PlayerAttackBaseState
{
    public PlayerParryState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine)
    {

    }

    ~PlayerParryState()
    {

    }
    protected override string p_animationTrigger => "Parry";
    protected override PlayerAttackConfig p_AttackConfig => p_context.Stats.CurrentAttackData.AttackConfig;


    public override void OnEnter()
    {
        p_context.Events.ParrySucceeded += OnParrySucceeded;
        base.OnEnter();
    }

    public override void OnUpdate()
    {
        if (!p_context.Health.IsDead && p_context.Stats.IsDamaged)
        {
            p_stateMachine.ChangeState<PlayerHitState>();
        }
        else
        {
            HandleInput();
        }
    }

    public override void OnExit()
    {
        p_context.Events.ParrySucceeded -= OnParrySucceeded;
        p_context.Stats.ClearParrySet();
        base.OnExit();

    }

    /// <summary>
    /// 공격 중 입력을 처리하여 다음 상태를 결정합니다.
    /// </summary>
    protected override void HandleInput()
    {
        if (p_nextState != null || !_canInput)
        {
            return;
        }

        if (p_context.Stats.CanNextAttack && p_context.Stamina.CheckStamina())
        {
            if (p_context.Input.AttackInput)
            {
                p_stateMachine.ChangeState(typeof(PlayerAttackState));
            }
            else if (p_context.Input.AttackHeldInput)
            {
                p_stateMachine.ChangeState(typeof(PlayerChargeState));
            }
            else if (p_context.Input.ParryInput)
            {
                p_stateMachine.ChangeState(typeof(PlayerParryState));
            }

        }
        else if (p_context.Input.DodgeInput && p_context.Stamina.CheckStamina())
        {

            p_nextState = typeof(PlayerDodgeState);
        }

    }


    /// <summary>
    /// 공격 판정이 발생하는 시점에 호출됩니다.
    /// </summary>
    protected override void OnAttackPerformed()
    {
        PlayerAttackConfig attackConfig = p_AttackConfig;
        attackConfig.AttackType = AttackType.NormalCounter;

        Collider[] colliders = p_context.Combat.ExecuteAttack(attackConfig);

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<IParryable>(out var parryable))
            {
                p_context.Stats.ParrySet.Add(parryable);
            }
            p_context.Events.TriggerChargeAttackAffected(collider);
        }

        p_context.Input.SetAttackHeldInput(false);
    }


    private void OnParrySucceeded(Transform transform)
    {
        if (transform.TryGetComponent<IDamageable>(out var damageable) && !damageable.IsDead)
        {
            damageable.TakeDamage(new DamageData(transform, p_AttackConfig.AttackType, p_AttackConfig.AttackDamage
                , p_AttackConfig.StiffnessAmount, p_AttackConfig.KnockBackCurve, p_AttackConfig.KnockBackDuration, p_AttackConfig.KnockBackForce));
        }
    }
}