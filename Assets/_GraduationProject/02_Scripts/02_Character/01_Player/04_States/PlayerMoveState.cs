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

        // 지면 체크: 공중에 떠 있으면 낙하 상태로 전환 (후한 판정 사용)
        if (!p_owner.Movement.IsGrounded())
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
    protected override void OnMove(Vector2 moveInput)
    {
        if (moveInput == Vector2.zero)
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
            return;
        }
    }

    #endregion

}