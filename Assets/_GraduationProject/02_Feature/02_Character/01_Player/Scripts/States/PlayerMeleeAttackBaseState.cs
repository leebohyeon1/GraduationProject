using System;
using System.Collections;
using BH_Lib.FSM;
using BH_Lib.Log;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 플레이어 근접 공격 상태의 기본 클래스
/// 모든 근접 공격 상태(첫 번째 공격, 두 번째 공격 등)가 상속받는 추상 클래스입니다.
/// 공격 애니메이션 실행, 콤보 입력 처리, 공격 전진 이동 등의 공통 로직을 제공합니다.
/// </summary>
public abstract class PlayerMeleeAttackBaseState : BaseState<PlayerContext>
{
    /// <summary>다음 상태를 저장할 변수</summary>
    protected Type p_nextState;
    /// <summary>입력 허용 플래그</summary>  
    protected bool p_canInput = false;
    /// <summary>공격 이동 코루틴 참조</summary>
    private Coroutine _attackMoveCoroutine;

    /// <summary>애니메이션 트리거 이름 (하위 클래스에서 구현)</summary>    
    protected abstract string p_animationTrigger { get; }
    /// <summary>다음 공격 상태 타입 (하위 클래스에서 구현)</summary>
    protected abstract Type p_nextAttackState { get; }

    /// <summary>
    /// 플레이어 공격 기본 상태 생성자
    /// </summary>
    public PlayerMeleeAttackBaseState (PlayerContext context, StateMachine<PlayerContext> stateMachine) 
        : base(context, stateMachine) {}

    /// <summary>
    /// 공격 상태 진입 시 호출
    /// 공격 애니메이션 실행, 방향 설정 등 초기화 수행
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();

        p_nextState = null; // 다음 상태 초기화
        p_canInput = true;    // 콤보 상태 초기화

        p_context.EventBus.OnAllowAttackInput += OnAttackAnimationEvent;
        p_context.EventBus.OnAttackFinished += OnAttackFinishedAnimationEvent;


        Log.Print("Player entered Attack state");
        p_context.Animator.SetTrigger(p_animationTrigger);  // 공격 애니메이션 실행

        // 공격 실행
        if (p_context.MeleeAttack != null)
        {
            var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
            var lookInput = p_context.Controller.LookInput;
            var mousePosition = p_context.Controller.MousePosition;
            p_context.MeleeAttack.TryAttack(deviceType, lookInput, mousePosition);
        }
        
        // 공격 시 전진 이동 실행
        StartAttackMovement();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        HandleInput();
    }

    public override void OnExit()
    {
        base.OnExit();
        p_context.Animator.ResetTrigger(p_animationTrigger);

        p_context.EventBus.OnAllowAttackInput -= OnAttackAnimationEvent;
        p_context.EventBus.OnAttackFinished -= OnAttackFinishedAnimationEvent;


        // 공격 이동 코루틴 정리
        if (_attackMoveCoroutine != null)
        {
            p_context.StopCoroutine(_attackMoveCoroutine);
            _attackMoveCoroutine = null;
        }

        if (p_nextState == null || !p_nextState.IsSubclassOf(typeof(PlayerMeleeAttackBaseState)))
        {
            p_context.MeleeAttack.ResetComboCount();
        }

        p_nextState = null;
        Log.Print("Player exited Attack state");
    }

    /// <summary>
    /// 입력 처리
    /// 공격 중 입력을 감지하여 다음 상태를 결정
    /// </summary>
    protected virtual void HandleInput()
    {
        // 다음 상태가 아직 결정되지 않았고 입력이 허용된 경우
        if (p_canInput)
        {
            // 공격 중 입력 감지하여 다음 상태 저장
            if (p_nextAttackState != null && p_context.Controller.AttackInput)
            {
                p_nextState = p_nextAttackState;
            }
            else if (p_context.Controller.DodgeInput && p_context.Movement.CanDodge())
            {
                p_nextState = typeof(PlayerDodgeState);
            }
            else if (p_context.Controller.DefendInput)
            {
                p_nextState = typeof(PlayerDefendState);
            }
            else if (p_context.Controller.AttackHeldInput)
            {
                p_nextState = typeof(PlayerMeleeAttackChargeState);
            }
            else if (p_context.Controller.RangedAttackInput)
            {
                p_nextState = typeof(PlayerRangedAttackChargeState);
            }


            if (p_nextState != null)
            {
                Log.PrintColor(Color.skyBlue, $"[PlayerAttackBaseState] 다음 상태: {p_nextState}");
            }
            
        }
    }
    
    /// <summary>
    /// 공격 애니메이션 이벤트 핸들러
    /// 공격 시작 시점에 호출되어 입력을 허용
    /// </summary>
    protected virtual void OnAttackAnimationEvent()
    {
    }

    /// <summary>
    /// 공격 애니메이션 이벤트 핸들러
    /// 공격이 완료되면 다른 상태로 전환
    /// </summary>
    protected virtual void OnAttackFinishedAnimationEvent()
    {
        p_context.StartCoroutine(CoChangeNextState());
    }
    
    /// <summary>
    /// 다음 상태로 전환하는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoChangeNextState()
    {
        yield return new WaitForSeconds( p_context.MeleeAttack.MeleeAttackData
            .AttackDelay ); // 약간의 딜레이 후에 상태 전환

        p_canInput = false;


        // 저장된 다음 상태로 전환
        if (p_nextState != null)
        {
            p_stateMachine.ChangeState(p_nextState);
        }
        else
        {
            // 아무 입력이 없었으면 Idle 상태로
            p_stateMachine.ChangeState<PlayerIdleState>();
        }
    }
    
    /// <summary>
    /// 공격 시 전진 이동 시작
    /// </summary>
    private void StartAttackMovement()
    {
        if (p_context.MeleeAttack?.MeleeAttackData == null) return;
        
        var attackData = p_context.MeleeAttack.MeleeAttackData;
        if (attackData.AttackMoveDistance <= 0) return;
        
        _attackMoveCoroutine = p_context.StartCoroutine(p_context.Movement.CoMoveForwardWithCurve(attackData.AttackMoveDistance, attackData.AttackMoveDuration, attackData.AttackMoveCurve));
    }
}
