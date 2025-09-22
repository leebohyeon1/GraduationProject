using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;

public class PlayerSecondCounterAttackState : PlayerAttackBaseState
{
    protected override string p_animationTrigger => "SecondCounterAttack";

    protected override Type p_nextAttackState => null;

    protected override PlayerAttackData p_AttackData => p_context.DataBase.RuntimeData.CombatData.CounterAttackDatas[0];


    public PlayerSecondCounterAttackState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnAttackFinish += HandleAttackFinish;
        p_context.Events.OnAttackPerform += HandleAttackPerform;

        p_nextState = null; // 다음 상태 초기화

        Log.Print("Player entered SecondCounterAttack state");
        p_context.Animator.SetTrigger(p_animationTrigger);  // 공격 애니메이션 실행
        p_context.Combat.SetupCombatCenter();

        // 공격 실행
        var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
        var moveInput = p_context.Controller.MoveInput;
        var mousePosition = p_context.Controller.MousePosition;
        p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);


        // 공격 시 전진 이동 실행
        StartAttackMovement();
    }


    public override void OnExit()
    {
        p_context.Events.OnAttackFinish -= HandleAttackFinish;
        p_context.Events.OnAttackPerform -= HandleAttackPerform;

        p_context.Animator.ResetTrigger(p_animationTrigger);

        DOTween.Kill(p_animationTrigger);

        p_nextState = null;
        Log.Print("Player exited SecondCounterAttack state");
    }

}
