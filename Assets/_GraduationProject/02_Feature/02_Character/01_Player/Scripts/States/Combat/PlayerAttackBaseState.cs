using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어의 모든 공격 상태의 기반이 되는 추상 클래스입니다.
/// </summary>
public abstract class PlayerAttackBaseState : BaseState<Player>
{
    protected Type p_nextState; // 다음 전환될 상태

    protected abstract string p_animationTrigger { get; } // 각 공격에 맞는 애니메이션 트리거
    protected abstract Type p_nextAttackState { get; } // 다음 연계 공격 상태
    protected abstract PlayerAttackData p_AttackData { get; } // 현재 공격의 데이터

    public PlayerAttackBaseState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnAttackFinish += HandleAttackFinish;
        p_context.Events.OnAttackPerform += HandleAttackPerform;

        p_nextState = null;

        p_context.Animator.SetTrigger(p_animationTrigger);
        p_context.Combat.SetupCombatCenter();

        // 목표 방향으로 회전
        if(p_context.Movement.HasTargetRotation)
        {
            p_context.Movement.RotateToTargetRotation();
        }
        else
        {
            var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
            var moveInput = p_context.Input.MoveInput;
            var mousePosition = p_context.Input.MousePosition;
            p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);
        }
            
        p_context.Events.TriggerBattleStateChanged(true);
          
        StartAttackMovement();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        HandleInput();
    }

    public override void OnExit()
    {
        p_context.Events.OnAttackFinish -= HandleAttackFinish;
        p_context.Events.OnAttackPerform -= HandleAttackPerform;

        p_context.Animator.ResetTrigger(p_animationTrigger);
        p_context.Events.TriggerBattleStateChanged(true);

        DOTween.Kill(p_animationTrigger);

        p_nextState = null;
    }

    /// <summary>
    /// 공격 애니메이션 종료 시 호출됩니다.
    /// </summary>
    protected virtual void HandleAttackFinish()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(p_AttackData.AttackDelay);

        // 다음 공격이 연계 공격이 아닐 경우 추가 딜레이
        if (p_nextState == null || !typeof(PlayerAttackBaseState).IsAssignableFrom(p_nextState))
        { 
            sequence.SetDelay(p_context.Stats.LastAttackDelay);
            p_context.Movement.ClearTargetRotation();
        }

        sequence.AppendCallback(() =>
        {
            if (p_nextState != null)
            {
                p_stateMachine.ChangeState(p_nextState);
            }
            else
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
            }
        });
    }

    /// <summary>
    /// 공격 판정이 발생하는 시점에 호출됩니다.
    /// </summary>
    protected virtual void HandleAttackPerform()
    {
        Collider[] colliders = p_context.Combat.ExecuteAttack(p_AttackData);
        foreach (Collider collider in colliders)
        {
            p_context.Events.TriggerAttackAffect(collider);
        }
    }

    /// <summary>
    /// 공격 시 앞으로 나아가는 움직임을 시작합니다.
    /// </summary>
    protected virtual void StartAttackMovement()
    {
        float distance = p_AttackData.AttackMoveDistance;

        Collider playerCollider = p_context.GetComponent<Collider>();

        // 전방에 장애물이 있으면 이동 거리 조정
        if (Physics.BoxCast(p_context.transform.position, playerCollider.bounds.extents * 1.2f,
            p_context.transform.forward, out var hitInfo,
            p_context.transform.rotation, 
            p_AttackData.AttackMoveDistance,
            p_context.Stats.BasePlayerDatasSO.CombatData.AttackLayerMask | p_context.Stats.BasePlayerDatasSO.ObstacleLayerMask))
        {
            distance = hitInfo.distance - (p_context.GetComponent<Collider>().bounds.size.z / 2);
        }

        if (distance <= 0) return;

        Vector3 moveDirection = p_context.transform.forward;
        float duration = p_AttackData.AttackMoveDuration;
        AnimationCurve curve = p_AttackData.AttackMoveCurve;

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
            duration)
            .SetEase(curve)
            .SetId(p_animationTrigger)
            .SetUpdate(UpdateType.Fixed);
    }

    /// <summary>
    /// 공격 중 입력을 처리하여 다음 상태를 결정합니다.
    /// </summary>
    protected virtual void HandleInput()
    {
        if (p_nextAttackState != null && p_context.Input.AttackInput)
        {
            var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
            var moveInput = p_context.Input.MoveInput;
            var mousePosition = p_context.Input.MousePosition;
            p_context.Movement.SetTargetRotation(p_context.Movement.GetTargetRotation(deviceType, moveInput, mousePosition));
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
}