using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;

/// <summary>
/// 플레이어의 첫 번째 카운터 공격 상태입니다.
/// </summary>
public class PlayerFirstCounterAttackState : PlayerAttackBaseState
{
    protected override string p_animationTrigger => "FirstCounterAttack";
    protected override Type p_nextAttackState => typeof(PlayerSecondCounterAttackState);
    protected override PlayerAttackData p_AttackData => p_context.Stats.CounterAttackDatas[0];

    public PlayerFirstCounterAttackState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnAttackFinish += HandleAttackFinish;
        p_context.Events.OnAttackPerform += HandleAttackPerform;
        p_context.Events.OnFirstCounterAttackExplosion += HandleFirstCounterAttackExplosion;

        p_nextState = null;

        p_context.Animator.SetTrigger(p_animationTrigger);

        p_context.Combat.SetCanCounterAttack(false);
        p_context.Stats.IsCounterAttack = true;

        StartAttackMovement();
        p_context.Events.TriggerFirstCounterAttackStart();
    }

    public override void OnExit()
    {
        p_context.Events.OnAttackFinish -= HandleAttackFinish;
        p_context.Events.OnAttackPerform -= HandleAttackPerform;
        p_context.Events.OnFirstCounterAttackExplosion -= HandleFirstCounterAttackExplosion;

        p_context.Animator.ResetTrigger(p_animationTrigger);

        DOTween.Kill(p_animationTrigger);

        p_nextState = null;
    }

    /// <summary>
    /// 공격 판정이 발생하는 시점에 호출됩니다.
    /// </summary>
    protected override void HandleAttackPerform()
    {
        p_context.Combat.ExcuteFirstCounterAttack(p_AttackData);
        p_context.Events.TriggerFirstCounterAttackAffect(p_context.Combat.CounterableTarget.GetComponent<Collider>());

        
    }

    /// <summary>
    /// 공격 애니메이션 종료 시 호출됩니다.
    /// </summary>
    protected override void HandleAttackFinish()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(p_AttackData.AttackDelay);

        sequence.AppendCallback(() =>
        {
            if (p_nextState != null)
            {
                // 다음 상태가 연계 카운터 공격이 아니면 카운터 상태 해제
                if (p_nextAttackState != p_nextState)
                {
                    p_context.Stats.IsCounterAttack = false;
                    p_context.Combat.ClearCounterTarget();
                }
                p_stateMachine.ChangeState(p_nextState);
            }
            else
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
            }
        });
    }

    /// <summary>
    /// 공격 중 입력을 처리하여 다음 상태를 결정합니다.
    /// </summary>
    protected override void HandleInput()
    {
        if (p_nextAttackState != null && p_context.Input.AttackInput && p_context.Stats.IsBoost)
        {
            p_nextState = p_nextAttackState;
        }
        else if (p_context.Input.DodgeInput && Time.time - p_context.Movement.LastDodgeTime >= p_context.Stats.BasePlayerDatasSO.CombatData.DodgeCooldown)
        {
            p_nextState = typeof(PlayerDodgeState);
        }
        else if (p_context.Input.DefendInput)
        {
            p_nextState = typeof(PlayerDefendState);
        }
        else if (p_context.Input.AttackHeldInput)
        {
            p_nextState = typeof(PlayerChargeState);
        }
        else if (p_context.Input.RangedAttackInput)
        {
            p_nextState = typeof(PlayerRangedChargeState);
        }
    }

    protected override void StartAttackMovement()
    {
        Vector3 targetPosition = new Vector3(p_context.Combat.CounterableTarget.transform.forward.x, p_context.transform.position.y, p_context.Combat.CounterableTarget.transform.forward.z);
        p_context.Movement.ForceMove(targetPosition);

        Quaternion targetRotation = Quaternion.LookRotation(-p_context.Combat.CounterableTarget.transform.forward);
        p_context.Movement.SetRotation(targetRotation);
    }

    private void HandleFirstCounterAttackExplosion()
    {
        Collider targetCollider = p_context.Combat.CounterableTarget.GetComponent<Collider>();
        p_context.Heat.IncreaseHeatOnCounterAttack(targetCollider);
        
        switch (p_context.Heat.CurrentTier)
        {
            case 1: p_context.Events.PlayFeedback(PlayerFeedbackType.Tier1CounterAttackFirstHit_FB, targetCollider.transform.position); break;
            case 2: p_context.Events.PlayFeedback(PlayerFeedbackType.Tier2CounterAttackFirstHit_FB, targetCollider.transform.position); break;
            case 3: p_context.Events.PlayFeedback(PlayerFeedbackType.Tier3CounterAttackFirstHit_FB, targetCollider.transform.position); break;
        }

        p_context.Heat.SetHeat(0);

        // 이동 시작

        float distance = p_AttackData.AttackMoveDistance;

        Collider playerCollider = p_context.GetComponent<Collider>();

        // 전방에 장애물이 있으면 이동 거리 조정
        if (Physics.BoxCast(p_context.transform.position, playerCollider.bounds.extents * 1.2f,
           -p_context.transform.forward, out var hitInfo,
           p_context.transform.rotation,
            p_AttackData.AttackMoveDistance,
            p_context.Stats.BasePlayerDatasSO.CombatData.AttackLayerMask | p_context.Stats.BasePlayerDatasSO.ObstacleLayerMask))
        {
            distance = hitInfo.distance + (p_context.GetComponent<Collider>().bounds.size.z / 2);
        }

        Vector3 moveDirection = -p_context.transform.forward;
        float duration = p_AttackData.AttackMoveDuration;
        AnimationCurve curve = p_AttackData.AttackMoveCurve;

        float currentDistance = 0f;
        DOTween.To(
            () => currentDistance,
            x =>
            {
                Vector3 displacement = moveDirection * (currentDistance - x);
                Log.Print(displacement);
                p_context.Movement.ForceMove(displacement);
                currentDistance = x;
            },
            distance,
            duration)
            .SetEase(curve)
            .SetId(p_animationTrigger)
            .SetUpdate(UpdateType.Fixed);
    }

}