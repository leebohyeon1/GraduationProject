using DG.Tweening;
using UnityEngine;

/// <summary>
/// 플레이어의 대기 상태입니다.
/// </summary>
public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(StateMachine<PlayerController> stateMachine)
        : base(stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();

        // 상태를 변경했을 때 이동 입력이 있는 상태면 이동 상태로 전환
        if (p_owner.InputHandler.MoveInput != Vector3.zero)
        {
            p_stateMachine.ChangeState<PlayerMoveState>();
        }
    }

    public override void OnFixedUpdate()
    {
        // 지면 체크: 공중에 떠 있으면 낙하 상태로 전환 (후한 판정 사용)
        if (!p_owner.Movement.IsGrounded())
        {
            p_stateMachine.ChangeState<PlayerFallingState>();
            return;
        }

        if (p_owner.LockOn.IsLockOn)
        {
            Vector3 targetPosition = new Vector3(p_owner.LockOn.CurrentTarget.position.x, 0, p_owner.LockOn.CurrentTarget.position.z);
            Vector3 directionToTarget = (targetPosition - new Vector3(p_owner.transform.position.x, 0, p_owner.transform.position.z)).normalized;

            p_owner.Movement.Rotate(directionToTarget, Time.fixedDeltaTime);
        }

        // 대기 상태에서는 움직이지 않음
        p_owner.Movement?.Move(Vector3.zero, Time.fixedDeltaTime);
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

        // 차지 레벨 초기화
        p_owner.Combat.SetCharge(false);
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        // 대기 애니메이션 재생 
        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.Idle);
    }
    #endregion

    #region Input
    /// <summary>
    /// 이동 입력 이벤트 처리
    /// </summary>
    /// <param name="moveInput">이동 입력</param>
    protected override void OnMove(Vector2 moveInput)
    {
        p_stateMachine.ChangeState<PlayerMoveState>();
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