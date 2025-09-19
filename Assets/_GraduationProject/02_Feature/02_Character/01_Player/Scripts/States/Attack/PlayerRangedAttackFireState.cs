using System;
using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어 원거리 공격 발사 상태
/// 차징이 완료된 후 투사체를 발사하는 상태입니다.
/// 발사 애니메이션 중 다른 입력을 받아 다음 상태를 결정할 수 있습니다.
/// </summary>
public class PlayerRangedAttackFireState : BaseState<PlayerContext>
{
    /// <summary>다음 상태를 저장할 변수</summary>
    private Type _nextState;
    
    /// <summary>입력 허용 플래그</summary>  
    protected bool p_canInput = false;

    /// <summary>
    /// 원거리 공격 발사 상태 생성자
    /// </summary>
    public PlayerRangedAttackFireState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
        : base(context, stateMachine) { }

    /// <summary>
    /// 발사 상태 진입 시 호출
    /// 발사 애니메이션 실행 및 투사체 발사
    /// </summary>
    public override void OnEnter()
    {
        p_context.Event.RangedAttack.OnFinished += OnRangedAttackEndEvent;
        p_context.Animator.SetTrigger("RangedAttackFire");
        Log.Print("Player entered RangedAttackFireState");

        // 투사체 발사
        FireProjectile();
    }

    /// <summary>
    /// 발사 상태 업데이트
    /// 발사 중 입력을 처리하여 다음 상태 결정
    /// </summary>
    public override void OnUpdate()
    {
        HandleInput();
    }

    /// <summary>
    /// 발사 상태 종료 시 호출
    /// 이벤트 구독 해제
    /// </summary>
    public override void OnExit()
    {
        p_context.Event.RangedAttack.OnFinished -= OnRangedAttackEndEvent;
        Log.Print("Player exited RangedAttackFireState");
    }

    /// <summary>
    /// 입력 처리
    /// 발사 중 입력을 감지하여 다음 상태를 결정
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
        else if (p_context.Controller.AttackHeldInput)
        {
            _nextState = typeof(PlayerMeleeAttackChargeState);
        }
        else if (p_context.Controller.AttackInput)
        {
            _nextState = typeof(PlayerFirstMeleeAttackState);
        }
        else if(p_context.Controller.MoveInput != Vector2.zero)
        {
            _nextState = typeof(PlayerMoveState);
        }

        if (_nextState != null)
        {
            Log.PrintColor(Color.skyBlue, $"[PlayerAttackBaseState] 다음 상태: {_nextState}");
        }
    }
    
    /// <summary>
    /// 투사체 발사 실행
    /// 이벤트 버스를 통해 발사 신호 전송
    /// </summary>
    private void FireProjectile()
    {
        p_context.Event.RangedAttack.PublishPerform();
    }

    /// <summary>
    /// 원거리 공격 애니메이션 종료 이벤트 핸들러
    /// 다음 상태로 전환하거나 Idle 상태로 복귀
    /// </summary>
    private void OnRangedAttackEndEvent()
    {
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

}