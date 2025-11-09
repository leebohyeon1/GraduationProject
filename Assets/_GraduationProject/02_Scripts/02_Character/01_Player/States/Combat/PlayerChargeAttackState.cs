using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;

/// <summary>
/// 플레이어의 차지 공격 상태입니다.
/// </summary>
public class PlayerChargeAttackState : PlayerAttackBaseState
{
    protected override string p_animationTrigger => "ChargeAttack";
    protected override Type p_nextAttackState => null;
    protected override PlayerAttackDataSO p_AttackData => p_context.Stats.Data.CombatData.ChargeAttackDatas[p_context.Stats.ChargeLevel].AttackData;

    public PlayerChargeAttackState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    /// <summary>
    /// 공격 판정이 발생하는 시점에 호출됩니다.
    /// </summary>
    protected override void HandleAttackPerform()
    {
        Collider[] colliders = p_context.Combat.ExecuteAttack(p_AttackData);

        foreach (Collider collider in colliders)
        {
            p_context.Events.TriggerChargeAttackAffect(collider);
        }

        p_context.Input.SetAttackHeldInput(false);
    }

    /// <summary>
    /// 공격 중 입력을 처리하여 다음 상태를 결정합니다.
    /// </summary>
    protected override void HandleInput()
    {
        if (p_nextState != null || !_canInput)
        {
            return;
        }

        if (p_nextAttackState != null && p_context.Input.AttackInput)
        {
            var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
            var moveInput = p_context.Input.MoveInput;
            var mousePosition = p_context.Input.MousePosition;
            p_context.Movement.SetTargetRotation(p_context.Movement.GetTargetRotation(deviceType, moveInput, mousePosition));
            p_nextState = p_nextAttackState;
            p_stateMachine.ChangeState(p_nextState);
        }
        else if (p_context.Input.DodgeInput)
        {
            p_nextState = typeof(PlayerDodgeState);
        }
        else if (p_context.Input.DefendInput)
        {
            p_nextState = typeof(PlayerDefendState);
        }
    }
}