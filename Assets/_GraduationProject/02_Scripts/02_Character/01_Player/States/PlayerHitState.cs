using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 플레이어의 피격 상태입니다.
/// </summary>
public class PlayerHitState : BaseState<Player>
{
    private float _hitDuration = 0.1f; // 피격 경직 시간
    private float _hitForce = 0f;
    private float _hitTimer; // 피격 시간 타이머

    public PlayerHitState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        _hitDuration = p_context.Health.StiffnessDuration;
        _hitForce = p_context.Health.KnockbackForce;

        // 피격 종류에 따라 다른 애니메이션 및 효과 재생
        if (p_context.Stats.IsKnockDown)
        {
            p_context.Events.TriggerTakeDamge(PlayerDamagedType.Strong);
            p_context.Animator.SetTrigger("KnockDownHit");
        }
        else if (p_context.Stats.IsHeavyHit)
        {
            KnockbackMovement(_hitForce * _hitDuration);

            p_context.Animator.SetTrigger("Hit");
            p_context.Events.TriggerTakeDamge(PlayerDamagedType.Strong);
        }
        else if(p_context.Stats.IsMiddleHit)
        {
            KnockbackMovement(_hitForce * _hitDuration);
              
            p_context.Animator.SetTrigger("Hit");
            p_context.Events.TriggerTakeDamge(PlayerDamagedType.Normal);
        }

        p_context.Animator.SetBool("IsHit", true);

        _hitTimer = 0f;

        p_context.Events.TriggerBattleStateChanged(true);
    }

    public override void OnUpdate()
    {
        _hitTimer += Time.deltaTime;

        p_context.Movement?.Move(Vector3.zero, 0f, 0f);

        // 경직 시간이 지나면 상태 전환
        if (_hitTimer >= _hitDuration)
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
            p_context.Combat.SetDefending(false);
        }
    }

    public override void OnExit()
    {
        p_context.Health.ResetDamageData();

        DOTween.Kill(this);
        p_context.Animator.SetBool("IsHit", false);
        p_context.Events.TriggerBattleStateChanged(true);
    }

    private void KnockbackMovement(float distance)
    {
        Vector3 moveDirection = (p_context.transform.position - p_context.Health.DamageData.AttackerTransform.position).normalized;

        float currentDistance = 0f;
        DOTween.To(
            () => currentDistance,
            x =>
            {
                Vector3 displacement = moveDirection * (x - currentDistance);
                p_context.Movement.ForceMove(displacement);
                currentDistance = x;
            },
            distance,
            _hitDuration)
            .SetEase(p_context.Health.DamageData.KnockbackCurve)
            .SetId(this)
            .SetUpdate(UpdateType.Fixed);

        Log.Print($"플레이어 넉백 이동: {moveDirection * distance}");
    }
}