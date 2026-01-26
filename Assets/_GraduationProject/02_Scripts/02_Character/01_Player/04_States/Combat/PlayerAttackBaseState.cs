using DG.Tweening;
using System;
using UnityEngine;


/// <summary>
/// 플레이어의 모든 공격 상태의 기반이 되는 추상 클래스입니다.
/// </summary>
public abstract class PlayerAttackBaseState : PlayerBaseState
{
    protected Type p_nextState; // 다음 전환될 상태

    protected abstract PlayerAttackConfig p_AttackConfig { get; } // 현재 공격의 데이터

    protected bool p_canBufferInput => p_owner.InputHandler.CanBufferInput;

    public PlayerAttackBaseState(StateMachine<PlayerController> stateMachine)
        : base(stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

    }

    public override void OnExit()
    {
        base.OnExit();

    }

    #region Setup Function
    protected override void SetupEvents()
    {
        base.SetupEvents();

        p_owner.Events.AttackStarted += OnAttackStarted;
        p_owner.Events.AttackPerformed += OnAttackPerformed;
        p_owner.Events.AttackFinished += OnAttackFinished;
    }

    protected override void SetupStats()
    {
        base.SetupStats();

        p_nextState = null; // 다음 상태 초기화

        p_owner.Stamina.UseStamina(p_AttackConfig.AttackStamina);       // 스테미나 사용
        p_owner.Events.TriggerRegenStamina(false);                      // 스테미나 재생성 불가
        p_owner.Events.TriggerBufferInputEnded();                       // 선입력 종료
        p_owner.Events.TriggerBattleStateChanged(true);                 // 전투 상태 On  
    }
    #endregion

    #region Clear Function
    protected override void ClearEvents()
    {
        base.ClearEvents();

        p_owner.Events.AttackStarted -= OnAttackStarted;
        p_owner.Events.AttackPerformed -= OnAttackPerformed;
        p_owner.Events.AttackFinished -= OnAttackFinished;
        DOTween.Kill(this);
    }

    protected override void ClearStats()
    {
        base.ClearStats();

        p_owner.Events.TriggerBattleStateChanged(true);     // 전투 상태 On
        p_owner.Events.TriggerBufferInputEnded();           // 선입력 종료
        p_owner.Events.TriggerRegenStamina(true);           // 스테미나 재생성

        p_nextState = null;                                 // 다음 상태 null 처리
    }
    #endregion

    #region Input
    /// <summary>
    /// 공격 입력 처리
    /// </summary>
    protected override void OnNormalAttack()
    {
        // 일반 공격이 가능하지 않으면 리턴
        if (!p_owner.Combat.CanNormalAttack())
        {
            return;
        }

        // 선입력 가능하면 공격 상태 변경
        if (p_nextState == null && p_canBufferInput)
        {
            p_stateMachine.ChangeState<PlayerNormalAttackState>();
        }
    }

    /// <summary>
    /// 회피 입력 처리
    /// </summary>
    protected override void OnDodge()
    {
        // 선입력 가능하면 다음 상태 선택
        if (p_nextState == null && p_canBufferInput)
        {
            p_nextState = typeof(PlayerDodgeState);
        }
    }

    /// <summary>
    /// 일반 상쇄 입력 처리
    /// </summary>
    protected override void OnNormalCounter()
    {
        if (p_nextState == null && p_canBufferInput)
        {
            p_stateMachine.ChangeState<PlayerNormalCounterState>();
        }
    }

    /// <summary>
    /// 차지 시작 입력 처리
    /// </summary>
    protected override void OnChargeStart()
    {
        base.OnChargeStart();

        if (p_nextState == null && p_canBufferInput)
        {
            p_stateMachine.ChangeState<PlayerChargeState>();
        }
    }

    #endregion

    #region EventHandle
    /// <summary>
    /// 공격 시작 시 호출
    /// </summary>
    protected virtual void OnAttackStarted()
    {
        AttackStep();
    }

    /// <summary>
    /// 공격 판정이 발생하는 시점에 호출
    /// </summary>
    protected virtual void OnAttackPerformed()
    {
        p_owner.Combat.ExecuteAttack(p_AttackConfig);
    }

    /// <summary>    
    /// 공격 종료 시 호출
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
    #endregion

    /// <summary>
    /// 공격 스텝 밟기
    /// </summary>
    protected virtual void AttackStep()
    {
        // 스텝 방향 기본적으로 정면으로 설정
        Vector3 stepDirection = p_owner.Movement.transform.forward;

        // 입력 기기에 따른 스텝 방향 계산
        InputDeviceType currentDeviceType = p_owner.InputHandler.CurrentInputDevice;
        if (currentDeviceType == InputDeviceType.KeyboardMouse)
        {
            Vector3 mousePosition = p_owner.InputHandler.MousePosition;
            stepDirection = p_owner.Movement.GetDirectionToMouse(mousePosition);
        }
        else if (currentDeviceType == InputDeviceType.Gamepad)
        {
            Vector3 moveInput = p_owner.InputHandler.MoveInput;
            stepDirection = p_owner.Movement.GetRelativeVectorToCamera(moveInput);
        }

        p_owner.Movement.Step(stepDirection, p_AttackConfig.AttackMoveConfig, this);
    }

}