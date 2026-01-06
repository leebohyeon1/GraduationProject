using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;

/// <summary>
/// 플레이어의 차지 공격 상태입니다.
/// </summary>
public class PlayerChargeAttackState : PlayerAttackBaseState
{
    protected override string p_animationTrigger => "ChargeAttack";
    protected override PlayerAttackConfig p_AttackConfig => p_context.Stats.CurrentChargeAttackData.AttackConfig;
     
    public PlayerChargeAttackState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) 
    {
        p_context.Events.ParrySucceeded += OnParrySucceeded;
    }

    ~PlayerChargeAttackState()
    {
        p_context.Events.ParrySucceeded -= OnParrySucceeded;
    }

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnExit()
    {
        p_context.Stats.ChargeLevel = 0;
        p_context.Stats.IsParring = false;
        p_context.Stats.ClearParrySet();
        base.OnExit();
    }

    /// <summary>
    /// 공격 판정이 발생하는 시점에 호출됩니다.
    /// </summary>
    protected override void OnAttackPerformed()
    {
        Collider[] colliders = p_context.Combat.ExecuteAttack(p_AttackConfig);

        foreach (Collider collider in colliders)
        {
            if(collider.TryGetComponent<IParryable>(out var parryable))
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
