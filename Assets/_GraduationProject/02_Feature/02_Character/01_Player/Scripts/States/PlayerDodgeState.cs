using BH_Lib.FSM;
using BH_Lib.Log;
using System;
using UnityEngine;


/// <summary>
/// 플레이어 대기 상태
/// 입력이 없을 때의 기본 상태
/// </summary>
public class PlayerDodgeState : BaseState<Player>
{
    private Vector3 _dodgeDirection;
    private Type _nextState;

    public PlayerDodgeState(Player context, StateMachine<Player> stateMachine)
    : base(context, stateMachine) { }

    public override void OnEnter()
    {
        _nextState = null; // 다음 상태 초기화

        p_context.Animator.SetTrigger("Dodge");
        p_context.Events.OnDodgeFinish += HandleDodgeFinish;

        if (p_context.Controller.MoveInput != Vector2.zero)
        {
            // PlayerMovement.Move()가 카메라 기준으로 변환하므로 입력 그대로 전달
            _dodgeDirection = new Vector3(p_context.Controller.MoveInput.x, 0, p_context.Controller.MoveInput.y);
            p_context.Movement.RotateToDirection(_dodgeDirection);
        }
        else
        {
            // 입력이 없으면 Dodge 함수에서 직접 처리하므로 방향 설정 필요 없음
            _dodgeDirection = Vector3.zero;
        }

        p_context.Health.SetInvisible(true);

        if(p_context.Combat.IsBattleState)
        {
            p_context.Events.TriggerBattleStateChanged(true);
        }

        p_context.Events.TriggerDodgeStart();
    }

    public override void OnUpdate()
    {
        HandleInput();
    }

    public override void OnFixedUpdate()
    {
        p_context.Movement?.Dodge(_dodgeDirection, 
            p_context.Stats.CombatData.DodgeSpeed);
    }

    public override void OnExit()
    {
        p_context.Events.OnDodgeFinish -= HandleDodgeFinish;

        p_context.Health.SetInvisible(false);

        if (p_context.Combat.IsBattleState)
        {
            p_context.Events.TriggerBattleStateChanged(true);
        }
    }

    /// <summary>
    /// 회피 애니메이션 종료 이벤트 핸들러
    /// </summary>
    public void HandleDodgeFinish()
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
        if (p_context.Controller.DefendInput)
        {
            _nextState = typeof(PlayerDefendState);
        }
        else if (p_context.Controller.AttackHeldInput)
        {
            _nextState = typeof(PlayerChargeState);
        }
        else if (p_context.Controller.AttackInput)
        {
            var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
            var moveInput = p_context.Controller.MoveInput;
            var mousePosition = p_context.Controller.MousePosition;
            p_context.Movement.SetTargetRotation(p_context.Movement.GetTargetRotation(deviceType, moveInput, mousePosition));

            _nextState = typeof(PlayerFirstAttackState);
        }
        else if (p_context.Controller.RangedAttackInput)
        {
            _nextState = typeof(PlayerRangedChargeState);
        }
        else if (p_context.Controller.MoveInput != Vector2.zero)
        {
            _nextState = typeof(PlayerMoveState);
        }
        else if(p_context.Controller.SkillInput)
        {
           // _nextState = typeof(PlayerSkillState);
        }

        if (_nextState != null)
        {
            Log.PrintColor(Color.skyBlue, $"[PlayerAttackBaseState] 다음 상태: {_nextState}");
        }
    }
}


