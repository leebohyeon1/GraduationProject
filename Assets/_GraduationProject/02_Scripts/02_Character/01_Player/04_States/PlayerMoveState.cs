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
        p_stateMachine.ChangeState<PlayerDodgeState>();
    }

    /// <summary>
    /// 공격 입력 이벤트 처리
    /// </summary>
    protected override void OnNormalAttack()
    {
        base.OnNormalAttack();

        p_stateMachine.ChangeState<PlayerNormalAttackState>();
    }

    /// <summary>
    /// 일반 상쇄 이벤트 처리
    /// </summary>
    protected override void OnNormalCounter()
    {
        base.OnNormalCounter();

        p_stateMachine.ChangeState<PlayerNormalCounterState>();
    }

    /// <summary>
    /// 차지 시작 이벤트 처리
    /// </summary>
    protected override void OnChargeStart()
    {
        base.OnChargeStart();

        p_stateMachine.ChangeState<PlayerChargeState>();
    }
    #endregion

}