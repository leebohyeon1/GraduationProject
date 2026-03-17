using UnityEngine;

/// <summary>
/// 플레이어의 이동 상태입니다.
/// </summary>
public class PlayerMoveState : PlayerBaseState
{
    public PlayerMoveState(StateMachine<PlayerController> stateMachine)
        : base(stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        // 지면 체크: 공중에 떠 있으면 낙하 상태로 전환
        if (!p_owner.GetComponent<CharacterController>().isGrounded)
        {
            p_stateMachine.ChangeState<PlayerFallingState>();
            return;
        }

        // 플레이어를 카메라 기준으로 이동
        p_owner.Movement.MoveByInput(p_owner.InputHandler.MoveInput, Time.fixedDeltaTime);
        p_owner.Movement.RotateToVelocity(Time.fixedDeltaTime);

        float moveSpeedRatio = p_owner.Movement.CurrentMoveSpeed / p_owner.Movement.MaxMoveSpeed;
        p_animator.SetFloat("MoveInput", moveSpeedRatio);
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    #region Setup Function
    protected override void SetupStats()
    {
        base.SetupStats();

        // 일반 공격 콤보 순서 초기화
        p_owner.Combat.ResetNormalAttackComboIndex();
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        // 이동 애니메이션 재생 
        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.Move);
    }
    #endregion

    #region Input
    /// <summary>
    /// 이동 입력 이벤트 처리
    /// </summary>
    /// <param name="moveInput">이동 입력</param>
    protected override void OnMove(Vector2 moveInput)
    {
        if (moveInput == Vector2.zero)
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
            return;
        }
    }

    /// <summary>
    /// 회피 입력 이벤트 처리
    /// </summary>
    protected override void OnDodge()
    {
        if (p_owner.Stamina.CheckStamina() && p_owner.Movement.CanDodge)
        {
            p_stateMachine.ChangeState<PlayerDodgeState>();
        }
    }

    /// <summary>
    /// 공격 입력 이벤트 처리
    /// </summary>
    protected override void OnNormalAttack()
    {
        base.OnNormalAttack();
        if (p_owner.Stamina.CheckStamina())
        {
            p_stateMachine.ChangeState<PlayerNormalAttackState>();
        }
    }

    /// <summary>
    /// 강공격 입력 이벤트 처리
    /// </summary>
    protected override void OnHeavyAttack()
    {
        base.OnHeavyAttack();
        
        // 패리 스택이 1개 이상이면 강공격 상태로 전환
        if (p_owner.Combat.ParryStacks > 0 && p_owner.Stamina.CheckStamina())
        {
            p_stateMachine.ChangeState<PlayerHeavyAttackState>();
        }
        else
        {
            // 패리 스택이 없으면 일반 공격 처리
            OnNormalAttack();
        }
    }

    /// <summary>
    /// 일반 상쇄 이벤트 처리
    /// </summary>
    protected override void OnNormalCounter()
    {
        base.OnNormalCounter();
        if (p_owner.Stamina.CheckStamina())
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

        if (p_owner.Stamina.CheckStamina())
        {
            p_stateMachine.ChangeState<PlayerChargeState>();
            return;
        }
    }
    #endregion

}