using BH_Lib.FSM;
using BH_Lib.Log;
using System.Collections;
using Unity.InferenceEngine;
using UnityEngine;

/// <summary>
/// 플레이어의 차지 상태입니다.
/// </summary>
public class PlayerChargeState : BaseState<Player>
{
    private int _chargeLevel => p_context.Stats.ChargeLevel;
    private float _chargeTimer = 0f;

    public PlayerChargeState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Animator.SetBool("IsCharge", true);

        p_context.Stats.ChargeLevel = 0;
        _chargeTimer = 0f;

        p_context.Events.TriggerChargeStart();
        p_context.Events.TriggerBattleStateChanged(true);

        var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
        var moveInput = p_context.Input.MoveInput;
        var mousePosition = p_context.Input.MousePosition;
        p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);

    }

    public override void OnUpdate()
    {
        _chargeTimer += Time.deltaTime;
        if(_chargeLevel < p_context.Stats.Data.CombatData.ChargeAttackDatas.Length  && 
            _chargeTimer >= p_context.Stats.Data.CombatData.ChargeAttackDatas[_chargeLevel].ChargeTime)
        {
            p_context.Stats.ChargeLevel++;
            p_context.Events.TriggerChargeLevelFeedback(_chargeLevel);
        }
        
        if(_chargeTimer >= p_context.Stats.Data.CombatData.MaxChargeTime)
        {
            p_stateMachine.ChangeState<PlayerChargeAttackState>();
        }

        p_context.Stamina.UseStamina(p_context.Stats.Data.CombatData.ChargeStamina * Time.deltaTime);

        if (p_context.Movement != null && p_context.Input.MoveInput != Vector2.zero)
        {
            Vector3 moveDirection = new Vector3(p_context.Input.MoveInput.x, 0, p_context.Input.MoveInput.y).normalized;
            p_context.Movement.Move(moveDirection, p_context.Stats.Data.CombatData.ChargeMoveSpeed, 0);
        }

        var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
        var moveInput = p_context.Input.MoveInput;
        var mousePosition = p_context.Input.MousePosition;
        p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);

        // 입력에 따른 상태 전환
        if (!p_context.Input.AttackHeldInput)
        {
            if (_chargeLevel > 0)
            {
                p_stateMachine.ChangeState<PlayerChargeAttackState>();
            }
            else
            {
                p_context.Stats.ChargeLevel = 0;
                p_stateMachine.ChangeState<PlayerFirstAttackState>();
            }
        }
    }

    public override void OnExit()
    {
        p_context.Animator.SetBool("IsCharge", false);
        p_context.Events.TriggerBattleStateChanged(true);
        p_context.Events.TriggerChargeCancel();
    }

}