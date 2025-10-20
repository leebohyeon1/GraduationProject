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
    protected override Type p_nextAttackState => null;
    protected override PlayerAttackData p_AttackData => p_context.Stats.CombatData.ChargeAttackData;

    private PlayerAttackData _playerAttackData;

    public PlayerChargeAttackState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnAttackFinish += HandleAttackFinish;
        p_context.Events.OnAttackPerform += HandleAttackPerform;

        p_nextState = null;

        p_context.Animator.SetTrigger(p_animationTrigger);

        _playerAttackData = p_AttackData;

        // 목표 방향으로 회전
        if (p_context.Movement.HasTargetRotation)
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

        StartAttackMovement();
        p_context.Events.TriggerChargeAttackStart(p_context.Heat.CurrentTier);
    }

    public override void OnExit()
    {
        p_context.Events.OnAttackFinish -= HandleAttackFinish;
        p_context.Events.OnAttackPerform -= HandleAttackPerform;

        p_context.Animator.ResetTrigger(p_animationTrigger);

        DOTween.Kill(p_animationTrigger);

        p_nextState = null;
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
                p_stateMachine.ChangeState(p_nextState);
            }
            else
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
            }
        });
        sequence.Play();
    }

    /// <summary>
    /// 공격 판정이 발생하는 시점에 호출됩니다.
    /// </summary>
    protected override void HandleAttackPerform()
    {
        Collider[] colliders = p_context.Combat.ExecuteAttack(_playerAttackData);

        foreach (Collider collider in colliders)
        {
            p_context.Events.TriggerChargeAttackAffect(collider);
        }
    }

    /// <summary>
    /// 공격 시 앞으로 나아가는 움직임을 시작합니다.
    /// </summary>
    protected override void StartAttackMovement()
    {
        float distance = p_AttackData.AttackMoveDistance;

        // 전방에 장애물이 있으면 이동 거리 조정
        if (Physics.Raycast(p_context.transform.position, p_context.transform.forward,
            out var hitInfo, p_AttackData.AttackMoveDistance,
            p_context.Stats.CombatData.AttackLayerMask & p_context.Stats.ObstacleLayerMask))
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
}