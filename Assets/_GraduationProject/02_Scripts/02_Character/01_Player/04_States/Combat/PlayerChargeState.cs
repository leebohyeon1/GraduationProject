using System;
using UnityEngine;

/// <summary>
/// 플레이어의 차지 상태입니다.
/// </summary>
public class PlayerChargeState : PlayerBaseState
{
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

        // 실시간으로 'Charge' 능력을 보유하고 있는지 확인 (능력이 사라지면 차지 취소)
        if (!p_owner.Ability.HasAbility("Charge"))
        {
            OnChargeCancel();
            return;
        }

        _chargeTimer += Time.deltaTime;

        // 차지가 안된 상태에서 차지 타이머가 차지 시간에 도달하면 차지 시작
        if (!p_owner.Combat.IsCharge && _chargeTimer >= p_owner.Combat.HeavyCounterAttackConfig.ChargeTime)
        {
            p_animator.SetTrigger("ChargeReady");
            p_owner.Combat.SetCharge(true);
            p_owner.Events.TriggerChargeCompleted(true);
        }

        if (_chargeTimer > p_owner.Combat.MaxChargeTime)
        {
            if (_isStep)
            {
                // 대시 중이면 예약만 하고 리턴
                _shouldTransition = true;
            }
            else
            {
                // 대시 중이 아니면 즉시 상태 전환
                p_stateMachine.ChangeState<PlayerHeavyCounterState>(); 
            }
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

        Vector3 moveDirection = Vector3.zero;
        // 기동차징 어빌리티가 있는 경우에만 이동 처리
        if (p_owner.Ability.HasAbility("MobileCharge"))
        {
            // 이동 방향 계산
            moveDirection = p_owner.Movement.GetRelativeVectorToCamera(p_owner.InputHandler.MoveInput);

            // 이동 처리 (차지 이동 속도 적용)
            p_owner.Movement.Move(moveDirection, p_owner.Movement.ChargeMoveSpeed, Time.fixedDeltaTime);
            Vector3 localMove = p_owner.transform.InverseTransformDirection(moveDirection);
            p_animator.SetFloat("X", localMove.x);
            p_animator.SetFloat("Y", localMove.z);
        }
        else
        {
            // 기동차징이 없으면 제자리 고정 (애니메이션 파라미터 초기화)
            p_animator.SetFloat("X", 0);
            p_animator.SetFloat("Y", 0);
        }

        // 회전 처리는 기동차징 여부와 상관없이 가능하도록 유지 (필요 시 조건 추가 가능)
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
        p_owner.Combat.ResetHeavyAttackComboIndex();       // 강공격 콤보 순서 초기화
        p_owner.Events.TriggerRegenStamina(false);                      // 스테미나 재생성 불가
        
        if (p_owner.Combat.IsCharge)
        {
            p_owner.Events.TriggerChargeCompleted(false);
        }
        p_owner.Combat.SetCharge(false);
        p_owner.Combat.TriggerBattleStateChanged(true);     // 전투 상태 On
       
        _chargeTimer = 0f;
        _shouldTransition = false;
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        p_animator.SetTrigger("Counter");
        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.Charge);
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

        p_owner.Combat.TriggerBattleStateChanged(true);
        p_owner.Events.TriggerRegenStamina(true);                      // 스테미나 재생성 가능

        _shouldTransition = false;
        _chargeTimer = 0f;
    }

    #endregion

    #region Input

    /// <summary>
    /// 공격 입력 처리
    /// </summary>
    protected override void OnNormalAttack() { }
    protected override void OnHeavyAttack() { }
    protected override void OnNormalCounter() { }
    protected override void OnChargeStart() { }

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

        if (p_owner.Combat.IsCharge)
        {
            p_stateMachine.ChangeState<PlayerHeavyCounterState>();
        }
        else
        {
            // 차지 레벨을 채우지 못한 레벨 초기화
            p_owner.Combat.SetCharge(false);

            p_stateMachine.ChangeState<PlayerNormalCounterState>();
        }
    }

    /// <summary>
    /// 구르기 입력 
    /// </summary>
    protected override void OnDodge()
    {
        // 기동차징 어빌리티가 있고, 대시 중이 아니며, 스테미나가 충분하고, 대시 쿨타임이 아닐 때
        if (p_owner.Ability.HasAbility("MobileCharge") && !_isStep && p_owner.Stamina.CheckStamina() && p_owner.Movement.CanDodge)
        {
            MobileChargeAbilitySO mobileCharge = p_owner.Ability.GetAbility("MobileCharge") as MobileChargeAbilitySO;
            if (mobileCharge == null) return;

            // 대시 방향 결정 (입력이 없으면 캐릭터 전방)
            Vector3 moveInput = p_owner.InputHandler.MoveInput;
            Vector3 dashDirection = moveInput.sqrMagnitude > 0.01f ? p_owner.Movement.GetRelativeVectorToCamera(moveInput) : p_owner.transform.forward;

            // 대시 데이터 설정 (StepData 구조체 사용)
            StepData dashData = new StepData
            {
                StepDistance = mobileCharge.DashDistance,
                StepDuration = mobileCharge.DashDuration,
                StepCurve = mobileCharge.DashCurve,
                StepRotateSpeed = p_owner.Movement.MaxRotateSpeed // 기존 회전 속도 사용
            };

            // 차징 상태를 유지하며 스텝(대시) 실행
            _isStep = true;
            p_owner.Movement.Step(dashDirection, dashData, this, false, OnStepComplete);

            // 대시 애니메이션 재생
            p_animator.CrossFade(mobileCharge.DashAnimationName, 0.1f);
            
            // 대시 종료 시간 기록 (쿨타임용)
            p_owner.Movement.SetLastDodgeEndTime();
            
            // 스테미나 소모 (필요 시 데이터에서 가져오도록 수정 가능)
            p_owner.Stamina.UseStamina(p_owner.Movement.DodgeConfig.StaminaConsumption.Value);
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
            if (p_owner.Combat.IsCharge)
            {
                p_stateMachine.ChangeState<PlayerHeavyCounterState>();
            }
            else
            {
                // 차지 레벨을 채우지 못한 레벨 초기화
                p_owner.Combat.SetCharge(false);

                p_stateMachine.ChangeState<PlayerNormalCounterState>();
            }
        }
    }

    #endregion
}
   