/// <summary>
/// PlayerController의 상태 머신(FSM) 초기화를 담당하는 부분
/// </summary>
public partial class PlayerController
{
    /// <summary>
    /// FSM 초기화 및 상태 등록
    /// </summary>
    private void InitializeFSM()
    {
        // 머신 초기화
        _stateMachine = new StateMachine<PlayerController>(this);

        // 모든 상태 등록
        RegisterStates();

        // 초기 상태 설정
        _stateMachine.ChangeState(typeof(PlayerIdleState));
    }

    /// <summary>
    /// 사용 가능한 모든 상태를 FSM에 등록합니다.
    /// 새로운 상태가 추가되면 여기에 등록해야 합니다.
    /// </summary>
    private void RegisterStates()
    {
        _stateMachine.AddState(new PlayerIdleState(_stateMachine));
        _stateMachine.AddState(new PlayerMoveState(_stateMachine));
        _stateMachine.AddState(new PlayerDodgeState(_stateMachine));
        _stateMachine.AddState(new PlayerNormalAttackState(_stateMachine));
        _stateMachine.AddState(new PlayerHeavyAttackState(_stateMachine));
        _stateMachine.AddState(new PlayerNormalCounterState(_stateMachine));
        _stateMachine.AddState(new PlayerHeavyCounterState(_stateMachine));
        _stateMachine.AddState(new PlayerChargeState(_stateMachine));
        _stateMachine.AddState(new PlayerDamagedState(_stateMachine));
        _stateMachine.AddState(new PlayerKnockdownState(_stateMachine));
        _stateMachine.AddState(new PlayerFallingState(_stateMachine));
        _stateMachine.AddState(new PlayerLandingState(_stateMachine));
    }
}
