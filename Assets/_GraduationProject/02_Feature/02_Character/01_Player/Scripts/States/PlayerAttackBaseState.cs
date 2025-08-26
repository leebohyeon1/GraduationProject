using System;
using System.Collections;
using BH_Lib.FSM;
using BH_Lib.Log;
using Unity.VisualScripting;
using UnityEngine;

public abstract class PlayerAttackBaseState : BaseState<PlayerContext>
{
    private Type _nextState; // 다음 상태를 저장할 변수
    protected bool p_canInput = false; // 입력 허용 플래그  
    private Coroutine _attackMoveCoroutine; // 공격 이동 코루틴 참조

    protected abstract string p_animationTrigger { get; }   // 애니메이션 트리거 이름    
    protected abstract Type p_nextAttackState { get; }  // 다음 공격 상태 타입

    /// <summary>
    /// 플레이어 공격 기본 상태 생성자
    /// </summary>
    public PlayerAttackBaseState (PlayerContext context, StateMachine<PlayerContext> stateMachine) 
        : base(context, stateMachine) {}

    /// <summary>
    /// 공격 상태 진입 시 호출
    /// 공격 애니메이션 실행, 방향 설정 등 초기화 수행
    /// </summary>
    public override void OnEnter()
    {
        base.OnEnter();

        _nextState = null; // 다음 상태 초기화
        p_canInput = true;    // 콤보 상태 초기화

        p_context.EventBus.OnAllowAttackInput += OnAttackAnimationEvent;
        p_context.EventBus.OnAttackFinished += OnAttackFinishedAnimationEvent;
        p_context.EventBus.OnAttack += p_context.Attack.PerformAttack;

        Log.Print("Player entered Attack state");
        p_context.Animator.SetTrigger(p_animationTrigger);  // 공격 애니메이션 실행

        // 공격 실행
        if (p_context.Attack != null)
        {
            var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
            var lookInput = p_context.Controller.LookInput;
            var mousePosition = p_context.Controller.MousePosition;
            p_context.Attack.TryAttack(deviceType, lookInput, mousePosition);
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
        p_context.EventBus.OnAttack -= p_context.Attack.PerformAttack;

        // 공격 이동 코루틴 정리
        if (_attackMoveCoroutine != null)
        {
            p_context.StopCoroutine(_attackMoveCoroutine);
            _attackMoveCoroutine = null;
        }

        if (_nextState == null || !_nextState.IsSubclassOf(typeof(PlayerAttackBaseState)))
        {
            p_context.Attack.ResetComboCount();
        }

        _nextState = null;
        Log.Print("Player exited Attack state");
    }

    /// <summary>
    /// 입력 처리
    /// 공격 중 입력을 감지하여 다음 상태를 결정
    /// </summary>
    private void HandleInput()
    {
        // 다음 상태가 아직 결정되지 않았고 입력이 허용된 경우
        if (p_canInput)
        {
            // 공격 중 입력 감지하여 다음 상태 저장
            if (p_nextAttackState != null && p_context.Controller.AttackInput)
            {
                _nextState = p_nextAttackState;
               
            }
            else if (p_context.Controller.DodgeInput && p_context.Movement.CanDodge())
            {
                _nextState = typeof(PlayerDodgeState);
            }

            if (_nextState != null)
            {
                Log.PrintColor(Color.red, $"[PlayerAttackBaseState] 다음 상태: {_nextState}");
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
        yield return new WaitForSeconds(0.15f); // 약간의 딜레이 후에 상태 전환

        p_canInput = false;

        yield return new WaitForSeconds(0.05f);

        // 저장된 다음 상태로 전환
        if (_nextState != null)
        {
            p_stateMachine.ChangeState(_nextState);
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
        if (p_context.Stats?.AttackData == null || p_context.Stats.AttackData.Length == 0) return;
        
        var attackData = p_context.Stats.AttackData[p_context.Attack.ComboCount];
        if (attackData.AttackMoveDistance <= 0) return;
        
        _attackMoveCoroutine = p_context.StartCoroutine(p_context.Movement.CoMoveForwardWithCurve(attackData.AttackMoveDistance, attackData.AttackMoveDuration, attackData.AttackMoveCurve));
    }
}
