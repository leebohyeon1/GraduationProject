using BH_Lib.FSM;
using BH_Lib.Log;
using System;
using UnityEngine;


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

        p_nextState = null; // 다음 상태 초기화
     
        p_context.Combat.SetupCombatCenter();

        // 목표 회전 값이 있을 경우 목표 회전값으로 회전 후 삭제
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

        if (p_context.Combat.CanCounterAttack && p_context.Combat.CanIsScanCounterable())
        {
            p_stateMachine.ChangeState<PlayerFirstCounterAttackState>();
        }
        else
        {
            p_context.Animator.SetTrigger(p_animationTrigger);  // 공격 애니메이션 실행
        }

        // 공격 시 전진 이동 실행
        StartAttackMovement();

        p_context.Events.TriggerFirstAttackStart();
    }

}

