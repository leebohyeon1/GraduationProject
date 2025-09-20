using System;
using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어 회피 상태
/// 회피 중에는 무적 프레임을 제공하고 빠른 이동을 수행
/// </summary>
public class PlayerDodgeState : BaseState<PlayerContext>
{
    private Vector3 _dodgeDirection;
    private Type _nextState;

    public PlayerDodgeState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
        p_context.Event.Dodge.OnFinished += HandleDodgeEndEvent;

        Log.Print("Player entered Dodge state");

        p_context.Animator.SetTrigger("Dodge");

        // 현재 이동 방향으로 회피, 입력이 없으면 앞쪽으로 회피
        if (p_context.Controller.MoveInput != Vector2.zero)
        {
            // PlayerMovement.Move()가 카메라 기준으로 변환하므로 입력 그대로 전달
            _dodgeDirection = new Vector3(p_context.Controller.MoveInput.x, 0, p_context.Controller.MoveInput.y);
            p_context.Movement.RotateImmediately(_dodgeDirection);
        }
        else
        {
            // 입력이 없으면 Dodge 함수에서 직접 처리하므로 방향 설정 필요 없음
            _dodgeDirection = Vector3.zero;
        }

        p_context.Event.Dodge.PublishStart(p_context.Owner.transform.position);
    }

    public override void OnUpdate()
    {
        // 회피 이동 실행
        if (p_context.Movement != null)
        {
            p_context.Movement.Dodge(_dodgeDirection);
        }

        HandleInput();
    }

    public override void OnExit()
    {
        p_context.Event.Dodge.OnFinished -= HandleDodgeEndEvent;

        Log.Print("Player exited Dodge state");
    }

    #region Feedback Handlers
    private void HandleDodgeEndEvent(Vector3 position)
    {
        OnDodgeEndEvent();
    }
    #endregion

    /// <summary>
    /// 회피 애니메이션 종료 이벤트 핸들러
    /// </summary>
    public virtual void OnDodgeEndEvent()
    {
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
    /// 입력 처리
    /// 회피 중 입력을 감지하여 다음 상태를 결정
    /// </summary>
    public void HandleInput()
    {
        if (p_context.Controller.DodgeInput && p_context.Movement.CanDodge())
        {
            _nextState = typeof(PlayerDodgeState);
        }
        else if (p_context.Controller.DefendInput)
        {
            _nextState = typeof(PlayerDefendState);
        }
        else if (p_context.Combat.CanCounterAttack && p_context.Controller.AttackInput)
        {
            _nextState = typeof(PlayerCounterAttackState);
        }
        else if (p_context.Controller.AttackHeldInput)
        {
            _nextState = typeof(PlayerMeleeAttackChargeState);
        }
        else if (p_context.Controller.AttackInput)
        {
            _nextState = typeof(PlayerFirstMeleeAttackState);
        }
        else if (p_context.Controller.RangedAttackInput)
        {
            _nextState = typeof(PlayerRangedAttackChargeState);
        }
        else if (p_context.Controller.MoveInput != Vector2.zero)
        {
            _nextState = typeof(PlayerMoveState);
        }

        if (_nextState != null)
        {
            Log.PrintColor(Color.skyBlue, $"[PlayerAttackBaseState] 다음 상태: {_nextState}");
        }
    }
    
}