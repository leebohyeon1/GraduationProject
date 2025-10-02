using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;


public class PlayerChargeAttackState : PlayerAttackBaseState
{
    protected override string p_animationTrigger => "ChargeAttack";

    protected override Type p_nextAttackState => null;

    protected override PlayerAttackData p_AttackData => p_context.Stats.CombatData.ChargeAttackData;

    private PlayerAttackData _playerAttackData = new PlayerAttackData();

    public PlayerChargeAttackState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnAttackFinish += HandleAttackFinish;
        p_context.Events.OnAttackPerform += HandleAttackPerform;

        p_nextState = null; // 다음 상태 초기화

        p_context.Animator.SetTrigger(p_animationTrigger);  // 공격 애니메이션 실행

        _playerAttackData = new PlayerAttackData();
        _playerAttackData = p_AttackData;

        // 목표 회전 값이 있을 경우 목표 회전값으로 회전 후 삭제
        if (p_context.Movement.HasTargetRotation)
        {
            p_context.Movement.RotateToTargetRotation();
        }
        else
        {
            var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
            var moveInput = p_context.Controller.MoveInput;
            var mousePosition = p_context.Controller.MousePosition;
            p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);
        }

        // 공격 시 전진 이동 실행
        StartAttackMovement();
        p_context.Events.TriggerChargeAttackStart();
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
    /// 공격 애니메이션 이벤트 핸들러
    /// 공격이 완료되면 다른 상태로 전환
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
    /// 공격 효과 적용
    /// </summary>
    protected override void HandleAttackPerform()
    {
        Collider[] colliders = p_context.Combat.ExecuteAttack(_playerAttackData);

        foreach (Collider collider in colliders)
        {
            p_context.Events.TriggerChargeAttackAffect(collider, p_context.Heat.CurrentTier);
        }

        p_context.Heat.SetHeat(0);
    }

    /// <summary>
    /// 공격 시 전진 이동 시작
    /// </summary>
    protected override void StartAttackMovement()
    {
        float distance = _playerAttackData.AttackMoveDistance;

        // 전방에 오브젝트가 있을 경우 전진 거리 조정
        if (Physics.Raycast(p_context.transform.position, p_context.transform.forward,
            out var hitInfo, _playerAttackData.AttackMoveDistance))
        {
            distance = hitInfo.distance - (p_context.GetComponent<Collider>().bounds.size.z / 2);

            _playerAttackData.AttackRadius.z = distance + 1;
        }

        Vector3 targetPosition = p_context.transform.position + (p_context.transform.forward * distance);

        p_context.transform.DOMove(targetPosition, p_AttackData.AttackMoveDuration, false)
        .SetEase(p_AttackData.AttackMoveCurve).SetId(p_animationTrigger);
    }

}
