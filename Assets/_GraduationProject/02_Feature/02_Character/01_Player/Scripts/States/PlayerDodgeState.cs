using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어 회피 상태
/// 회피 중에는 무적 프레임을 제공하고 빠른 이동을 수행
/// </summary>
public class PlayerDodgeState : BaseState<Player>
{
    private PlayerMovement _playerMovement;
    private PlayerController _playerController;
    private PlayerStats _stats;
    
    private float _dodgeDuration = 0.3f; // 회피 지속 시간
    private float _dodgeTimer = 0f;
    private Vector3 _dodgeDirection;
    private bool _isInvincible = false;

    public PlayerDodgeState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine)
    {
        _playerMovement = context.PlayerMovement;
        _playerController = context.PlayerController;
        _stats = context.PlayerStats;
    }

    public override void OnEnter()
    {
        Debug.Log("Player entered Dodge state");
        _dodgeTimer = 0f;
        _isInvincible = true;

        // 현재 이동 방향으로 회피, 입력이 없으면 앞쪽으로 회피
        if (_playerController.MoveInput != Vector2.zero)
        {
            // PlayerMovement.Move()가 카메라 기준으로 변환하므로 입력 그대로 전달
            _dodgeDirection = new Vector3(_playerController.MoveInput.x, 0, _playerController.MoveInput.y);
        }
        else
        {
            // 입력이 없으면 앞쪽 방향
            _dodgeDirection = new Vector3(p_context.transform.forward.x, 0, p_context.transform.forward.z);
        }

        _playerMovement.RotateImmediately(_dodgeDirection);
        
        // TODO: 무적 상태 활성화 (IDamageable 인터페이스 확장 필요)
        // SetInvincible(true);
    }

    public override void OnUpdate()
    {
        _dodgeTimer += Time.deltaTime;

        // 회피 이동 실행
        if (_playerMovement != null)
        {
            _playerMovement.Move(_dodgeDirection, _playerMovement.DodgeSpeed);
        }

        // 회피 완료 시 상태 전환
        if (_dodgeTimer >= _dodgeDuration)
        {
            // 이동 입력이 있으면 Move 상태로
            if (_playerController.MoveInput != Vector2.zero)
            {
                p_stateMachine.ChangeState<PlayerMoveState>();
                return;
            }

            // 공격 입력이 있으면 Attack 상태로
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
        Debug.Log("Player exited Dodge state");
        _dodgeTimer = 0f;
        _isInvincible = false;

        // TODO: 무적 상태 비활성화
        // SetInvincible(false);
    }
}