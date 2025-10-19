using BH_Lib.FSM;
using BH_Lib.Log;
using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 플레이어의 첫 번째 일반 공격 상태입니다.
/// </summary>
public class PlayerFirstAttackState : PlayerAttackBaseState
{
    public PlayerFirstAttackState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) { }

    protected override string p_animationTrigger => "FirstAttack";
    protected override Type p_nextAttackState => typeof(PlayerSecondAttackState);
    protected override PlayerAttackData p_AttackData => p_context.Stats.CombatData.AttackDatas[0];

    public override void OnEnter()
    {
        p_context.Events.OnAttackFinish += HandleAttackFinish;
        p_context.Events.OnAttackPerform += HandleAttackPerform;

        p_nextState = null;
     
        p_context.Combat.SetupCombatCenter();

        // 목표 방향으로 회전
        if (p_context.Movement.HasTargetRotation)
        {
            p_context.Movement.RotateToTargetRotation();
        }
        else
        {
            var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
            var moveInput = p_context.Input.MoveInput;
            var mousePosition = p_context.Input.MousePosition;
            p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);
        }

        p_context.Events.TriggerBattleStateChanged(true);

        // 카운터 공격이 가능한 경우 카운터 상태로 전환
        // 카운터 가능 상태를 굳이 나한테 필요없을 지도
        if (!p_context.Heat.IsOverHeat && p_context.Combat.CanCounterAttack && p_context.Combat.CanIsScanCounterable())
        {
            p_stateMachine.ChangeState<PlayerFirstCounterAttackState>();
        }
        else
        {
            p_context.Animator.SetTrigger(p_animationTrigger);
        }

        StartAttackMovement();
        p_context.Events.TriggerFirstAttackStart();
    }
}