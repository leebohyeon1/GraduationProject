using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 플레이어의 모든 공격 상태의 기반이 되는 추상 클래스입니다.
/// </summary>
public abstract class PlayerAttackBaseState : BaseState<Player>
{
    protected Type p_nextState; // 다음 전환될 상태
    protected bool _canInput = false;

    protected abstract string p_animationTrigger { get; } // 각 공격에 맞는 애니메이션 트리거
    protected abstract Type p_nextAttackState { get; } // 다음 연계 공격 상태
    protected abstract PlayerAttackDataSO p_AttackData { get; } // 현재 공격의 데이터

    public PlayerAttackBaseState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnAttackFinish += HandleAttackFinish;
        p_context.Events.OnAttackPerform += HandleAttackPerform;
        p_context.Events.OnAttackInputWindowStart += HandleAttackInputWindowStart;

        _canInput = false;
        p_nextState = null;
        p_context.Stamina.UseStamina(p_AttackData.AttackStamina);

        // p_context.Animator.runtimeAnimatorController = p_AttackData.AnimOverrideController;
        p_context.Animator.SetTrigger(p_animationTrigger);
            
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
        p_context.Events.OnAttackInputWindowStart -= HandleAttackInputWindowStart;
        DOTween.Kill(p_animationTrigger);

        p_context.Animator.ResetTrigger(p_animationTrigger);
        p_context.Events.TriggerBattleStateChanged(true);
        p_context.Events.TriggerRegenStamina(true);

        _canInput = false;  
        p_nextState = null;
    }


    /// <summary>
    /// 공격 시 앞으로 나아가는 움직임을 시작합니다.
    /// </summary>
    protected virtual void StartAttackMovement()
    {
        float distance = p_AttackData.AttackMoveDistance;
        float duration = p_AttackData.AttackMoveDuration;
        AnimationCurve curve = p_AttackData.AttackMoveCurve;

        float currentDistance = 0f;
        DOTween.To(
            () => currentDistance,
            x =>
            {
                if (p_context.Stats.IsLockOn)
                {
                    Vector3 targetPosition = new Vector3(p_context.LockOnSystem.CurrentTarget.position.x, 0, p_context.LockOnSystem.CurrentTarget.position.z);
                    Vector3 directionToTarget = (targetPosition - new Vector3(p_context.transform.position.x, 0, p_context.transform.position.z)).normalized;

                    p_context.Movement.SetRotation(Quaternion.LookRotation(directionToTarget), p_AttackData.RotateSpeed);
                }
                else
                {
                    var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
                    var moveInput = p_context.Input.MoveInput;
                    var mousePosition = p_context.Input.MousePosition;

                    p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition, p_AttackData.RotateSpeed);
                }

                Vector3 moveDirection = p_context.transform.forward;
                float deltaDistance = x - currentDistance;
                Vector3 displacement = moveDirection * deltaDistance;

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
        if (p_nextState != null || !_canInput)
        {
            return;
        }

        if (p_nextAttackState != null && p_context.Input.AttackInput && p_context.Stamina.CheckStamina())
        {
            p_nextState = p_nextAttackState;
            p_stateMachine.ChangeState(p_nextState);
        }
        else if (p_context.Input.DodgeInput && p_context.Stamina.CheckStamina())
        {
            p_nextState = typeof(PlayerDodgeState);
        }
        //else if (p_context.Input.AttackHeldInput && p_context.Stamina.CheckStamina())
        //{
        //    p_nextState = typeof(PlayerChargeState);
        //}
    }



    /// <summary>
    /// 공격 판정이 발생하는 시점에 호출됩니다.
    /// </summary>
    protected virtual void HandleAttackPerform()
    {
        DOTween.Kill(p_animationTrigger);

        Collider[] colliders = p_context.Combat.ExecuteAttack(p_AttackData);
        foreach (Collider collider in colliders)
        {
            p_context.Events.TriggerAttackAffect(collider);
        }
    }

    protected virtual void HandleAttackInputWindowStart()
    {
        _canInput = true;
    }

    /// <summary>    
    /// 공격 애니메이션 종료 시 호출됩니다.  
    /// </summary>
    protected virtual void HandleAttackFinish()
    {
        if (p_nextState != null)
        {
            p_stateMachine.ChangeState(p_nextState);
        }
        else
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
        }
    }

}