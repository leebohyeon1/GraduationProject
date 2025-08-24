using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어 회피 상태
/// 회피 중에는 무적 프레임을 제공하고 빠른 이동을 수행
/// </summary>
public class PlayerDodgeState : BaseState<Player>
{
    private Vector3 _dodgeDirection;
    private bool _isInvincible = false;

    public PlayerDodgeState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
        p_context.PlayerAnimationEventHandler.OnDodgeEnd += OnDodgeEndEvent;
        
        Debug.Log("Player entered Dodge state");

        p_context.PlayerAnimator.SetTrigger("Dodge");
        _isInvincible = true;

        // 현재 이동 방향으로 회피, 입력이 없으면 앞쪽으로 회피
        if (p_context.PlayerController.MoveInput != Vector2.zero)
        {
            // PlayerMovement.Move()가 카메라 기준으로 변환하므로 입력 그대로 전달
            _dodgeDirection = new Vector3(p_context.PlayerController.MoveInput.x, 0, p_context.PlayerController.MoveInput.y);
            p_context.PlayerMovement.RotateImmediately(_dodgeDirection);
        }
        else
        {
            // 입력이 없으면 Dodge 함수에서 직접 처리하므로 방향 설정 필요 없음
            _dodgeDirection = Vector3.zero;
        }

        // TODO: 무적 상태 활성화 (IDamageable 인터페이스 확장 필요)
        // SetInvincible(true);
    }

    public override void OnUpdate()
    {
        // 회피 이동 실행
        if (p_context.PlayerMovement != null)
        {
            bool hasInput = p_context.PlayerController.MoveInput != Vector2.zero;
            p_context.PlayerMovement.Dodge(_dodgeDirection, hasInput);
        }
    }

    public override void OnExit()
    {
        p_context.PlayerAnimationEventHandler.OnDodgeEnd -= OnDodgeEndEvent;

        Debug.Log("Player exited Dodge state");
        _isInvincible = false;

        // TODO: 무적 상태 비활성화
        // SetInvincible(false);
    }
    
    /// <summary>
    /// 회피 애니메이션 종료 이벤트 핸들러
    /// </summary>
    public virtual void OnDodgeEndEvent()
    {
        // 회피 완료 시 상태 전환
        // 이동 입력이 있으면 Move 상태로
        if (p_context.PlayerController.MoveInput != Vector2.zero)
        {
            p_stateMachine.ChangeState<PlayerMoveState>();
            return;
        }

        // 공격 입력이 있으면 Attack 상태로
        if (p_context.PlayerController.AttackInput)
        {
            p_stateMachine.ChangeState<PlayerFirstAttackState>();
            return;
        }

        // 아무 입력이 없으면 Idle 상태로
        p_stateMachine.ChangeState<PlayerIdleState>();
    }
}