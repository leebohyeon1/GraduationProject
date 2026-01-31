using System;
using UnityEngine;

/// <summary>
/// 플레이어의 차지 상태입니다.
/// </summary>
public class PlayerChargeState : PlayerBaseState
{
    private int _chargeLevel => p_owner.Combat.ChargeLevel;
    private float _chargeTimer = 0f;

    private bool _isStep;           // 차지 대시 중인가
    private bool _shouldTransition; // 상태 전환해야 하는지 여부

    public PlayerChargeState(StateMachine<PlayerController> stateMachine)
        : base(stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();

        Debug.Log("Enter Charge State");
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        _chargeTimer += Time.deltaTime;

        // 차지 레벨이 차지 카운터 리스트 갯수보다 작고, 다음 차지 시간이 지났으면
        if (_chargeLevel < (p_owner.Combat.HeavyCounterAttackConfigList.Count - 1) &&
            _chargeTimer >= p_owner.Combat.HeavyCounterAttackConfigList[_chargeLevel + 1].ChargeTime)
        {
            p_owner.Combat.IncreaseChargeLevel();
            p_owner.Events.TriggerChargeLevelCompleted(_chargeLevel);
        }

        if (_chargeTimer > p_owner.Combat.MaxChargeTime)
        {
            p_stateMachine.ChangeState<PlayerHeavyCounterState>(); 
        }

        // 매초마다 스테미나 감소
        p_owner.Stamina.UseStamina(p_owner.Combat.ChargeStamina * Time.deltaTime);
        if (!p_owner.Stamina.CheckStamina())
        {
            OnChargeCancel();
            return;
        }

        p_owner.Events.TriggerRegenStamina(false);                      // 스테미나 재생성 불가
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        // 이동 방향 계산
        Vector3 moveDirection = p_owner.Movement.GetRelativeVectorToCamera(p_owner.InputHandler.MoveInput);

        // 이동 처리
        p_owner.Movement.Move(moveDirection, p_owner.Movement.ChargeMoveSpeed, Time.fixedDeltaTime);
        Vector3 localMove = p_owner.transform.InverseTransformDirection(moveDirection);
        p_animator.SetFloat("X", localMove.x);
        p_animator.SetFloat("Y", localMove.z);

        // 회전 처리
        if (p_owner.LockOn.IsLockOn)
        {
            Vector3 targetPosition = new Vector3(p_owner.LockOn.CurrentTarget.position.x, 0, p_owner.LockOn.CurrentTarget.position.z);
            Vector3 directionToTarget = (targetPosition - new Vector3(p_owner.transform.position.x, 0, p_owner.transform.position.z)).normalized;

            p_owner.Movement.Rotate(directionToTarget, Time.fixedDeltaTime);
        }
        else
        {
            if (p_owner.InputHandler.CurrentInputDevice == InputDeviceType.Gamepad)
            {
                // 게임패드: 이동 방향으로 회전
                if (p_owner.InputHandler.MoveInput.sqrMagnitude > 0.01f)
                {
                    p_owner.Movement.Rotate(moveDirection, p_owner.Movement.ChargeRoataeSpeed, Time.fixedDeltaTime);
                }
            }
            else
            {
                // 키보드/마우스: 마우스 방향으로 회전
                Vector3 mouseDirection = p_owner.Movement.GetDirectionToMouse(p_owner.InputHandler.MousePosition);
                p_owner.Movement.Rotate(mouseDirection, p_owner.Movement.ChargeRoataeSpeed, Time.fixedDeltaTime);
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();

    }

    #region Setup Function
    protected override void SetupEvents()
    {
        base.SetupEvents();

        p_owner.Events.DodgeStarted += OnDodgeStarted;
        p_owner.Events.DodgeFinished += OnDodgeFinished;
    }

    protected override void SetupStats()
    {
        base.SetupStats();

        p_owner.Combat.ResetNormalAttackComboIndex();       // 일반 공격 콤보 순서 초기화
        p_owner.Events.TriggerRegenStamina(false);                      // 스테미나 재생성 불가
        p_owner.Combat.ResetChargeLevel();
        p_owner.Events.TriggerBattleStateChanged(true);     // 전투 상태 On
        _chargeTimer = 0f;
        _shouldTransition = false;

        p_owner.AnimationTrigger.ChargeCanceled();
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.Charge);
        p_animator.SetTrigger("ChargeStart");
    }
    #endregion

    #region Clear Function
    protected override void ClearEvents()
    {
        base.ClearEvents();

        p_owner.Events.DodgeStarted -= OnDodgeStarted;
        p_owner.Events.DodgeFinished -= OnDodgeFinished;
    }

    protected override void ClearStats()
    {
        base.ClearStats();

        p_owner.Events.TriggerBattleStateChanged(true);
        p_owner.Events.TriggerChargeFinshed();
        p_owner.Events.TriggerRegenStamina(true);                      // 스테미나 재생성 가능
        p_owner.AnimationTrigger.ChargeCanceled();  // 차지 종료 피드백

        _shouldTransition = false;
        _chargeTimer = 0f;
    }

    #endregion

    #region Input
    /// <summary>
    /// 차지 종료 입력
    /// </summary>
    protected override void OnChargeCancel()
    {
        base.OnChargeCancel();

        // 상태 전환해야 하면 리턴
        if (_shouldTransition)
        {
            return;
        }

        if (_isStep)
        {
            // 상태 전환해야 하는 상태로 전환
            _shouldTransition = true;
            return;
        }

        if (p_owner.Combat.ChargeLevel >= 0)
        {
            p_stateMachine.ChangeState<PlayerHeavyCounterState>();
        }
        else
        {
            // 차지 레벨을 채우지 못한 레벨 초기화
            p_owner.Combat.ResetChargeLevel();

            p_stateMachine.ChangeState<PlayerNormalCounterState>();
        }
    }

    /// <summary>
    /// 구르기 입력 
    /// </summary>
    protected override void OnDodge()
    {
        base.OnDodge();

        // Clash 기술이 있으면 차지 대시 가능
        if (p_owner.Stamina.CheckStamina() && p_owner.Ability.HasAbility("Clash"))
        {
            ClashSO clashSO = p_owner.Ability.GetAbility("Clash") as ClashSO;

            Vector3 moveInput = p_owner.InputHandler.MoveInput;
            Vector3 stepDirection = p_owner.Movement.GetRelativeVectorToCamera(moveInput);

            DodgeData ChargeDashData = clashSO.ChargeStepTagSO.DodgeConfig;
            StepData stepData = ChargeDashData.MoveConfig;

            p_owner.Movement.Step(stepDirection, stepData, this, false, OnStepComplete);
            // 1번 레이어에서 차지 대시 애니메이션 재생
            p_animator.Play(ChargeDashData.AnimationStateName, 1, 0f);
        }
    }

    #endregion

    #region EventHandler

    private void OnDodgeStarted()
    {
        _isStep = true;
    }

    private void OnDodgeFinished()
    {
        _isStep = false;
    } 

    private void OnStepComplete()
    {
        p_owner.Events.TriggerDodgeFinished();

        if(_shouldTransition)
        {
            if (p_owner.Combat.ChargeLevel >= 0)
            {
                p_stateMachine.ChangeState<PlayerHeavyCounterState>();
            }
            else
            {
                // 차지 레벨을 채우지 못한 레벨 초기화
                p_owner.Combat.ResetChargeLevel();

                p_stateMachine.ChangeState<PlayerNormalCounterState>();
            }
        }
    }

    #endregion
}
   