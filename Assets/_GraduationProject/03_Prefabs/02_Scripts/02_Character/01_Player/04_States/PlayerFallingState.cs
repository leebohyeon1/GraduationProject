using UnityEngine;

/// <summary>
/// 플레이어가 공중에 떠 있는 상태입니다. (공중 제어 포함)
/// </summary>
public class PlayerFallingState : PlayerBaseState
{
    private float _startFallHeight;
    private Vector2 _currentMoveInput;

    public PlayerFallingState(StateMachine<PlayerController> stateMachine)
        : base(stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
        
        // 낙하 시작 높이 기록
        _startFallHeight = p_owner.transform.position.y;
        
        // 현재 입력 핸들러의 값을 초기값으로 설정
        _currentMoveInput = new Vector2(p_owner.InputHandler.MoveInput.x, p_owner.InputHandler.MoveInput.z);
    }

    public override void OnFixedUpdate()
    {
        // 1. 공중 이동 처리 (기존 MoveByInput 활용)
        // 공중에서는 평소 속도의 약 30% 정도로만 움직이게 설정 (수치 조절 가능)
        float airControlMultiplier = 0.6f;
        Vector3 airMoveInput = new Vector3(_currentMoveInput.x, 0, _currentMoveInput.y) * airControlMultiplier;
        
        p_owner.Movement.MoveByInput(airMoveInput, Time.fixedDeltaTime);
        
        // 2. 공중 회전 처리 (이동 방향을 바라보게 함)
        p_owner.Movement.RotateToVelocity(Time.fixedDeltaTime);

        // 3. 지면에 닿았는지 확인
        if (p_owner.GetComponent<CharacterController>().isGrounded)
        {
            float fallDistance = _startFallHeight - p_owner.transform.position.y;

            // 3m 이상 높이에서 떨어졌을 때만 착지 동작 수행
            if (fallDistance >= 2.0f)
            {
                p_stateMachine.ChangeState<PlayerLandingState>();
            }
            else
            {
                // 낮게 떨어졌을 때는 입력 여부에 따라 Idle 또는 Move로 전환
                if (_currentMoveInput != Vector2.zero)
                    p_stateMachine.ChangeState<PlayerMoveState>();
                else
                    p_stateMachine.ChangeState<PlayerIdleState>();
            }
        }
    }

    /// <summary>
    /// 공중에서도 이동 입력을 계속 받도록 설정
    /// </summary>
    protected override void OnMove(Vector2 moveInput)
    {
        _currentMoveInput = moveInput;
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();
        // 낙하 애니메이션 재생
        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.Falling);
    }

    // 공격, 회피 등 다른 입력은 여전히 무시 (오버라이드하지 않음)
}
