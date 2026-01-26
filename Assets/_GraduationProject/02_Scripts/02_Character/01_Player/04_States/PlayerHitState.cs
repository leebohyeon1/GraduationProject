using DG.Tweening;
using UnityEngine;

/// <summary>
/// 플레이어의 피격 상태입니다.
/// </summary>
public class PlayerHitState : PlayerBaseState
{
    private float _hitDuration = 0.1f; // 피격 경직 시간
    private float _hitForce = 0f;
    private float _hitTimer; // 피격 시간 타이머

    public PlayerHitState(StateMachine<PlayerController> stateMachine) 
        : base(stateMachine) { }

    public override void OnEnter()
    {
        // 오류 방지용 초기화
        //p_owner.Stats.ResetNormalAttackComboIndex();
        //p_animator.SetInteger("ComboIndex", p_owner.Stats.NormalAttackComboIndex);
        //p_owner.Stats.IsCharge = false;

        //_hitDuration = p_owner.Health.StiffnessDuration;
        //_hitForce = p_owner.Health.KnockbackForce;

        //// 피격 종류에 따라 다른 애니메이션 및 효과 재생
        //if (p_context.Stats.IsKnockDown)
        //{
        //    p_context.Events.TriggerTakeDamged(PlayerDamagedType.KnockDown);
        //    p_context.Animator.SetTrigger("KnockDownHit");
        //}
        //else if (p_context.Stats.IsHeavyHit)
        //{
        //    KnockbackMovement(_hitForce * _hitDuration);

        //    p_context.Animator.SetTrigger("Hit");
        //    p_context.Events.TriggerTakeDamged(PlayerDamagedType.Strong);
        //}
        //else if(p_context.Stats.IsMiddleHit)
        //{
        //    KnockbackMovement(_hitForce * _hitDuration);

        //    p_context.Animator.SetTrigger("Hit");
        //    p_context.Events.TriggerTakeDamged(PlayerDamagedType.Normal);
        //}

        p_animator.SetBool("IsHit", true);

        _hitTimer = 0f;

        p_owner.Events.TriggerBattleStateChanged(true);
    }

    public override void OnUpdate()
    {
        _hitTimer += Time.deltaTime;

        p_owner.Movement?.Move(Vector3.zero, 0f, 0f);

        // 경직 시간이 지나면 상태 전환
        if (_hitTimer >= _hitDuration)
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
        }
    }

    public override void OnExit()
    {
        DOTween.Kill(this);
        p_animator.SetBool("IsHit", false);
        p_owner.Events.TriggerBattleStateChanged(true);
    }

    private void KnockbackMovement(float distance)
    {
        //Vector3 moveDirection = (p_owner.transform.position - p_owner.Health.DamageData.AttackerTransform.position).normalized;

        //p_owner.Movement.Step(moveDirection, )
        //float currentDistance = 0f;
        //DOTween.To(
        //    () => currentDistance,
        //    x =>
        //    {
        //        Vector3 displacement = moveDirection * (x - currentDistance);
        // (displacement);
        //        currentDistance = x;
        //    },
        //    distance,
        //    _hitDuration)
        //    .SetEase(p_owner.Health.DamageData.KnockbackCurve)
        //    .SetId(this)
        //    .SetUpdate(UpdateType.Fixed);
    }
}