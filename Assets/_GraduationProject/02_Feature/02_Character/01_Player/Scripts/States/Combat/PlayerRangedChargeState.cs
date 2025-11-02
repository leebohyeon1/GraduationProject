using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 원거리 공격 차지 상태입니다.
/// </summary>
public class PlayerRangedChargeState : BaseState<Player>
{
    private bool _isCharged = false; // 차지 완료 여부
    private float _chargeTimer; // 차지 시간 타이머

    public PlayerRangedChargeState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Animator.SetBool("IsRangedAttackCharging", true);
        _chargeTimer = 0f;
        _isCharged = false;

        p_context.Events.TriggerBattleStateChanged(true);
        p_context.Events.TriggerRangedChargeStart();
    }

    public override void OnUpdate()
    {
        p_context.Movement?.Move(Vector3.zero, 0f, 0f);

        _chargeTimer += Time.deltaTime;
        if (!_isCharged && _chargeTimer > p_context.Stats.RangedAttackData.ChargeTime)
        {
            p_context.Events.TriggerRangedChargeFinish();
            _isCharged = true;
        }

        // 입력에 따른 상태 전환
        if (!p_context.Input.RangedAttackInput)
        {
            if (_isCharged)
            {
                p_stateMachine.ChangeState<PlayerRangedAttackState>();
            }
            else
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
            }
        }
        else if (p_context.Input.DodgeInput)
        {
            p_stateMachine.ChangeState<PlayerDodgeState>();
        }

        // 조준 방향으로 회전
        var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
        var moveInput = p_context.Input.MoveInput;
        var mousePosition = p_context.Input.MousePosition;
        p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);
    }

    public override void OnExit()
    {
        p_context.Events.TriggerRangedChargeCancel();
        p_context.Animator.SetBool("IsRangedAttackCharging", false);
        p_context.Events.TriggerBattleStateChanged(true);
    }
}