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

    public override void OnUpdate()
    {   
        //if (!p_context.Health.IsDead && p_context.Stats.IsDamaged)
        //{
        //    p_stateMachine.ChangeState<PlayerHitState>();
        //}
        //else if (p_context.Input.MoveInput != Vector2.zero)
        //{
        //    p_stateMachine.ChangeState<PlayerMoveState>();
        //}
        //else if(p_context.Input.DodgeInput && p_context.Stamina.CheckStamina())
        //{
        //    p_stateMachine.ChangeState<PlayerDodgeState>();
        //}
        //else if (p_context.Input.AttackInput && p_context.Stamina.CheckStamina())
        //{
        //    p_stateMachine.ChangeState<PlayerAttackState>();
        //}
        //else if (p_context.Input.AttackHeldInput && p_context.Stamina.CheckStamina())
        //{
        //    p_stateMachine.ChangeState<PlayerChargeState>();
        //}
        //else if (p_context.Input.ParryInput && p_context.Stamina.CheckStamina())
        //{
        //    p_stateMachine.ChangeState <PlayerNormalCounterState>();
        //}
    }

    public override void OnFixedUpdate()
    {
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
        if (p_owner.Combat.NormalAttackComboIndex != -1)
        {
            p_owner.Combat.ResetNormalAttackComboIndex();
        }
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
        p_stateMachine.ChangeState<PlayerDodgeState>();
    }

    /// <summary>
    /// 공격 입력 이벤트 처리
    /// </summary>
    protected override void OnAttack()
    {
        base.OnAttack();

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