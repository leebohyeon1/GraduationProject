using UnityEngine;

/// <summary>
/// 플레이어가 착지 애니메이션을 수행 중인 상태입니다.
/// </summary>
public class PlayerLandingState : PlayerBaseState
{
    public PlayerLandingState(StateMachine<PlayerController> stateMachine)
        : base(stateMachine) 
    {
        p_owner.Events.Landed += OnLanded;
    }

    ~PlayerLandingState()
    {
        p_owner.Events.Landed -= OnLanded;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        
        // 착지 중 물리 이동 멈춤
        p_owner.Movement.Move(Vector3.zero, Time.fixedDeltaTime);
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();
        // 착지 애니메이션 재생 (State 10)
        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.Landing);
    }

    private void OnLanded()
    {
        p_stateMachine.ChangeState<PlayerIdleState>();
    }

    // 입력 이벤트 오버라이드하지 않음 (이동, 공격 등 차단)
}
