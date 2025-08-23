using BH_Lib.FSM;
using UnityEngine;

/// <summary>
/// 플레이어 공격 상태
/// 공격 중에는 이동이 제한되며, 공격이 완료되면 다른 상태로 전환
/// </summary>
public class PlayerAttackState : BaseState<Player>
{
    private PlayerAttack _playerAttack;
    private PlayerController _playerController;
    private float _attackDuration = 0.5f; // 공격 지속 시간
    private float _attackTimer = 0f;
    private bool _attackExecuted = false;

    public PlayerAttackState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine)
    {
        _playerAttack = context.PlayerAttack;
        _playerController = context.PlayerController;
    }

    public override void OnEnter()
    {
        Debug.Log("Player entered Attack state");
        _attackTimer = 0f;
        _attackExecuted = false;

        // 공격 실행
        if (_playerAttack != null)
        {
            _playerAttack.TryAttack();
            _attackExecuted = true;
        }
    }

    public override void OnUpdate()
    {
        _attackTimer += Time.deltaTime;

        // 공격이 완료되면 상태 전환 확인
        if (_attackTimer >= _attackDuration)
        {
            // 이동 입력이 있으면 Move 상태로
            if (_playerController.MoveInput != Vector2.zero)
            {
                p_stateMachine.ChangeState<PlayerMoveState>();
                return;
            }

            // 연속 공격 입력이 있으면 다시 Attack 상태로
            if (_playerController.AttackInput)
            {
                p_stateMachine.ChangeState<PlayerAttackState>();
                return;
            }

            // 아무 입력이 없으면 Idle 상태로
            p_stateMachine.ChangeState<PlayerIdleState>();
        }
    }

    public override void OnExit()
    {
        Debug.Log("Player exited Attack state");
        _attackTimer = 0f;
        _attackExecuted = false;
    }
}