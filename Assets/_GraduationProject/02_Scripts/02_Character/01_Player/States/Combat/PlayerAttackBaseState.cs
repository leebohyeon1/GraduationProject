using DG.Tweening;
using System;
using UnityEngine;


/// <summary>
/// 플레이어의 모든 공격 상태의 기반이 되는 추상 클래스입니다.
/// </summary>
public abstract class PlayerAttackBaseState : State<Player>
{
    protected Type p_nextState; // 다음 전환될 상태
    protected bool _canInput = false;

    protected abstract string p_animationTrigger { get; } // 각 공격에 맞는 애니메이션 트리거
    protected abstract PlayerAttackConfig p_AttackConfig { get; } // 현재 공격의 데이터

    public PlayerAttackBaseState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) 
    {
    }


    public override void OnEnter()
    {
        p_context.Events.AttackFinished += OnAttackFinished;
        p_context.Events.AttackPerformed += OnAttackPerformed;
        p_context.Events.AttackInputWindowStarted += OnAttackInputWindowStarted;

        _canInput = false;
        p_nextState = null;
        p_context.Stamina.UseStamina(p_AttackConfig.AttackStamina);

        p_context.Animator.SetTrigger(p_animationTrigger);
            
        p_context.Events.TriggerBattleStateChanged(true);
        
        StartAttackMovement();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

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
        p_context.Events.AttackFinished -= OnAttackFinished;
        p_context.Events.AttackPerformed -= OnAttackPerformed;
        p_context.Events.AttackInputWindowStarted -= OnAttackInputWindowStarted;
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
        float distance = p_AttackConfig.AttackMoveDistance;
        float duration = p_AttackConfig.AttackMoveDuration;
        AnimationCurve curve = p_AttackConfig.AttackMoveCurve;

        float currentDistance = 0f;
        DOTween.To(
            () => currentDistance,
            x =>
            {
                if (p_context.Stats.IsLockOn)
                {
                    Vector3 targetPosition = new Vector3(p_context.LockOnSystem.CurrentTarget.position.x, 0, p_context.LockOnSystem.CurrentTarget.position.z);
                    Vector3 directionToTarget = (targetPosition - new Vector3(p_context.transform.position.x, 0, p_context.transform.position.z)).normalized;

                    p_context.Movement.SetRotation(Quaternion.LookRotation(directionToTarget), p_AttackConfig.RotateSpeed);
                }
                else
                {
                    var deviceType = p_context.DeviceDetector.CurrentInputDevice;
                    var moveInput = p_context.Input.MoveInput;
                    var mousePosition = p_context.Input.MousePosition;

                    p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition, p_AttackConfig.RotateSpeed);
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
        
        if (p_context.Stats.CanNextAttack && p_context.Stamina.CheckStamina())
        {
            if (p_context.Stats.AttackComboIndex < 2 && p_context.Input.AttackInput)
            {
                p_context.Events.TriggerChangedNextAttackState();
                p_stateMachine.ChangeState(typeof(PlayerAttackState));
            }
            else if(p_context.Input.AttackHeldInput)
            {
                if(p_context.Stats.AttackComboIndex < 2)
                {
                    p_context.Events.TriggerChangedNextAttackState();
                }
                p_stateMachine.ChangeState(typeof(PlayerChargeState));
            }
            else if(p_context.Input.ParryInput)
            {
                if (p_context.Stats.AttackComboIndex < 2)
                {
                    p_context.Events.TriggerChangedNextAttackState();
                }
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
    protected virtual void OnAttackPerformed()
    {
        DOTween.Kill(p_animationTrigger);

        Collider[] colliders = p_context.Combat.ExecuteAttack(p_AttackConfig);
        foreach (Collider collider in colliders)
        {
            p_context.Events.TriggerAttackAffected(collider);
        }

    }

    protected virtual void OnAttackInputWindowStarted()
    {
        _canInput = true;
    }

    /// <summary>    
    /// 공격 애니메이션 종료 시 호출됩니다.  
    /// </summary>
    protected virtual void OnAttackFinished()
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