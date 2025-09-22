using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;


public class PlayerRangedChargeState : BaseState<Player>
{
    private bool _isCharged = false;

    private float _chargeTimer;

    public PlayerRangedChargeState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Animator.SetBool("IsRangedAttackCharging", true);
        _chargeTimer = 0f;

        p_context.Events.TriggerBattleStateChanged(true);

        Log.Print("Player entered RangedAttackChargeState");
    }

    public override void OnUpdate()
    {
        p_context.Movement?.Move(Vector3.zero, 0f, 0f);

        _chargeTimer += Time.deltaTime;
        if (!_isCharged && _chargeTimer > p_context.DataBase.RuntimeData.CombatData.RangedAttackData.ChargeTime)
        {
            p_context.Events.TriggerRangedChargeFinish();

            _isCharged = true;
        }

        if (!p_context.Controller.RangedAttackInput)
        {
            if (_isCharged)
            {
                p_stateMachine.ChangeState<PlayerRangedAttackState>();
                return;
            }
            else
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
                return;
            }
        }
        else if (p_context.Controller.DodgeInput)
        {
            p_stateMachine.ChangeState<PlayerDodgeState>();
        }

        // 에임 방향으로 회전
        var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
        var moveInput = p_context.Controller.MoveInput;
        var mousePosition = p_context.Controller.MousePosition;
        p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);
    }

    /// <summary>
    /// 차징 상태 종료 시 호출
    /// 차징 애니메이션 중지
    /// </summary>
    public override void OnExit()
    {
        p_context.Animator.SetBool("IsRangedAttackCharging", false);
        p_context.Events.TriggerBattleStateChanged(true);

        Log.Print("Player exited RangedAttackChargeState");
    }

}
