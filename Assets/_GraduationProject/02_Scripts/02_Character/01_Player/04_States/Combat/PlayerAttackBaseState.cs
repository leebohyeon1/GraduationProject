using DG.Tweening;
using System;
using UnityEngine;


/// <summary>
/// 플레이어의 모든 공격 상태의 기반이 되는 추상 클래스입니다.
/// </summary>
public abstract class PlayerAttackBaseState : PlayerBaseState
{
    protected Type p_nextState; // 다음 전환될 상태

    protected abstract IRuntimeAttackConfig p_AttackConfig { get; } // 현재 공격의 런타임 데이터

    protected bool p_canBufferInput => p_owner.InputHandler.CanBufferInput;
    protected bool p_canChangeCombatState = false;
    protected bool p_isAttackPerformed = false; // 현재 애니메이션의 공격 시작 이벤트 수신 여부

    public PlayerAttackBaseState(StateMachine<PlayerController> stateMachine)
        : base(stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
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
        p_owner.Events.ChangeNextCombatState += OnChangeNextCombatState;
        p_owner.Combat.CounterStackChanged += OnCounterStackChanged;
    }

    protected override void SetupStats()
    {
        base.SetupStats();

        p_nextState = null; // 다음 상태 초기화
        p_isAttackPerformed = false; // 상태 진입 시 플래그 초기화
        p_owner.Stamina.UseStamina(p_AttackConfig.Stamina.Value);       // 스테미나 사용
        p_owner.Events.TriggerRegenStamina(false);                      // 스테미나 재생성 불가
        p_owner.Events.TriggerBufferInputEnded();                       // 선입력 종료
        p_canChangeCombatState = false;
    }
    #endregion

    #region Clear Function
    protected override void ClearEvents()
    {
        base.ClearEvents();

        p_owner.Events.AttackStarted -= OnAttackStarted;
        p_owner.Events.AttackPerformed -= OnAttackPerformed;
        p_owner.Events.AttackFinished -= OnAttackFinished;
        p_owner.Events.ChangeNextCombatState -= OnChangeNextCombatState;
        p_owner.Combat.CounterStackChanged -= OnCounterStackChanged;
        DOTween.Kill(this);
    }

    protected override void ClearStats()
    {
        base.ClearStats();

        p_owner.Events.TriggerBufferInputEnded();           // 선입력 종료
        p_owner.Events.TriggerRegenStamina(true);           // 스테미나 재생성
        p_canChangeCombatState = false;
        p_isAttackPerformed = false;                          // 플래그 초기화
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

        if (p_nextState != null)
        {
            return;
        }

        if (!p_owner.Stamina.CheckStamina())
        {
            return;
        }

        if (p_canChangeCombatState)
        {
            p_stateMachine.ChangeState<PlayerNormalAttackState>();
        }
        else if (p_canBufferInput)
        {
            p_nextState = typeof(PlayerNormalAttackState);
        }
    }

    /// <summary>
    /// 강공격 입력 처리
    /// </summary>
    protected override void OnHeavyAttack()
    {
        if (p_nextState != null)
        {
            return;
        }

        // "HeavyAttack" 능력이 있고 패리 스택이 1개 이상일 때만 전환
        if (p_owner.Ability.HasAbility("HeavyAttack") && p_owner.Combat.CounterStacks > 0)
        {
            if (!p_owner.Stamina.CheckStamina())
            {
                return;
            }

            if (p_canChangeCombatState)
            {
                p_stateMachine.ChangeState<PlayerHeavyAttackState>();
            }
            else if (p_canBufferInput)
            {
                p_nextState = typeof(PlayerHeavyAttackState);
            }
        }
        else
        {
            // 능력이 없거나 패리 스택이 없으면 일반 공격 처리
            OnNormalAttack();
        }
    }

    /// <summary>
    /// 일반 상쇄 입력 처리
    /// </summary>
    protected override void OnNormalCounter()
    {

        if (p_nextState != null)
        {
            return;
        }

        if (p_owner.Stamina.CheckStamina())
        {
            if (p_canChangeCombatState)
            {
                p_stateMachine.ChangeState<PlayerNormalCounterState>();
            }
            else if (p_canBufferInput)
            {
                p_nextState = typeof(PlayerNormalCounterState);
            }
        }
    }

    /// <summary>
    /// 차지 시작 입력 처리
    /// </summary>
    protected override void OnChargeStart()
    {
        // "Charge" 능력이 없으면 리턴
        if (!p_owner.Ability.HasAbility("Charge"))
        {
            return;
        }

        if (p_nextState != null)
        {
            return;
        }
        if (!p_owner.Stamina.CheckStamina())
        {
            return;
        }

        if (p_canChangeCombatState)
        {
            p_stateMachine.ChangeState<PlayerChargeState>();
        }
        else if (p_canBufferInput)
        {
            p_nextState = typeof(PlayerChargeState);
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
        p_isAttackPerformed = true; // 현재 애니메이션의 시작 이벤트 확인
    }

    /// <summary>    
    /// 공격 종료 시 호출
    /// </summary>
    protected virtual void OnAttackFinished()
    {
        // 이벤트 수신 여부와 상관없이 다음 상태가 예약되어 있다면 전이 허용 (Failsafe 대응)
        if (!p_isAttackPerformed)
        {
            return;
        }
        
        p_isAttackPerformed = false;

        if (p_nextState != null)
        {
            p_stateMachine.ChangeState(p_nextState);
        }
        else
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
        }
    }

    protected virtual void OnChangeNextCombatState()
    {
        if (!p_isAttackPerformed)
        {
            return;
        }

        p_canChangeCombatState = true;

        if (p_nextState == null)
        {
            return;
        }

        bool isAttackState = typeof(PlayerAttackBaseState).IsAssignableFrom(p_nextState);
        bool isDodgeState = p_nextState == typeof(PlayerDodgeState);
        bool isChargeState = p_nextState == typeof(PlayerChargeState);
        bool isHeavyAttackState = p_nextState == typeof(PlayerHeavyAttackState);
        
        if (isAttackState || isDodgeState || isChargeState || isHeavyAttackState)
        {
            p_stateMachine.ChangeState(p_nextState);
        }
    }
    #endregion

    /// <summary>
    /// 공격 스텝 밟기
    /// </summary>
    protected virtual void AttackStep()
    {
        // 스텝 방향 기본적으로 정면으로 설정
        Vector3 stepDirection = p_owner.transform.forward;

        // 1. 락온 상태라면 락온 타겟 방향으로 설정 (최우선)
        if (p_owner.LockOn.IsLockOn && p_owner.LockOn.CurrentTarget != null)
        {
            stepDirection = (p_owner.LockOn.CurrentTarget.position - p_owner.transform.position).normalized;
            stepDirection.y = 0; // 높이 차이 무시
        }
        else
        {
            // 2. 락온 상태가 아닐 때 입력 기기에 따른 스텝 방향 계산
            InputDeviceType currentDeviceType = p_owner.InputHandler.CurrentInputDevice;
            if (currentDeviceType == InputDeviceType.KeyboardMouse)
            {
                Vector3 mousePosition = p_owner.InputHandler.MousePosition;
                stepDirection = p_owner.Movement.GetDirectionToMouse(mousePosition);
            }
            else if (currentDeviceType == InputDeviceType.Gamepad)
            {
                Vector3 moveInput = p_owner.InputHandler.MoveInput;
                
                // 입력이 일정 수치 이상일 때만 해당 방향으로 스텝, 아니면 정면(transform.forward) 유지
                if (moveInput.sqrMagnitude > 0.1f)
                {
                    stepDirection = p_owner.Movement.GetRelativeVectorToCamera(moveInput);
                }
                else
                {
                    stepDirection = p_owner.transform.forward;
                }
            }
        }

        p_owner.Movement.Step(stepDirection, p_AttackConfig.AttackMoveConfig, this, true);
    }

    private void OnCounterStackChanged(int currentStack)
    {
        p_AttackConfig.Damage.RemoveAllModifiersFromSource("CounterStack");

        StatModifier NormalCounterModifier = new StatModifier(p_owner.RuntimeData.CounterStackDamageMultipliers[currentStack].Value,
            StatModifierType.PercentAdd, "CounterStack");
        p_AttackConfig.Damage.AddModifier(NormalCounterModifier);
    }
}
