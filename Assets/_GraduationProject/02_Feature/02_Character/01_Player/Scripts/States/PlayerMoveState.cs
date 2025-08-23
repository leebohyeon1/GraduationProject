using BH_Lib.FSM;
using UnityEngine;

/// <summary>
/// 플레이어 이동 상태
/// 이동 입력이 있을 때 활성화되는 상태
/// </summary>
public class PlayerMoveState : BaseState<Player>
{
    private PlayerMovement _playerMovement;
    private PlayerController _playerController;

    public PlayerMoveState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine)
    {
        _playerMovement = context.PlayerMovement;
        _playerController = context.PlayerController;
    }

    public override void OnEnter()
    {
        Debug.Log("Player entered Move state");
    }

    public override void OnUpdate()
    {
        // 이동 처리 (상태 전환은 StateMachine의 조건부 전환으로 자동 처리됨)
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (_playerMovement != null && _playerController.MoveInput != Vector2.zero)
        {
            // 2D 입력을 3D 월드 좌표로 변환
            Vector3 moveDirection = new Vector3(_playerController.MoveInput.x, 0, _playerController.MoveInput.y);
            _playerMovement.Move(moveDirection);
        }
    }

    public override void OnExit()
    {
        Debug.Log("Player exited Move state");
    }
}