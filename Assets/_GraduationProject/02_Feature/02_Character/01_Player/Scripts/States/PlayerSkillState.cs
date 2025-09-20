using System;
using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

public class PlayerSkillState : BaseState<PlayerContext>
{
    private Type _nextState;
    
    private int _costMana; 

    public PlayerSkillState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
        : base(context, stateMachine) { }
    public override void OnEnter()
    {
        p_context.Animator.SetTrigger("Skill");

        _costMana = p_context.Heat.GetCostMana("OnSkillSuccess");
        if (_costMana <= p_context.Combat.CurrentMana)
        {
            p_context.Event.Skill.PublishStart(p_context.Combat.SkillEffectPoint.position);
            p_context.Event.Skill.OnFinished += SkillEndEvent;
        }
        else
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
        }

        Log.Print("Player entered Skill state");
    }

    public override void OnUpdate()
    {
        // 이동 처리 (상태 전환은 StateMachine의 조건부 전환으로 자동 처리됨)
        HandleInput();
        HandleMovement();
    }

    public override void OnExit()
    {
        p_context.Event.Skill.OnFinished -= SkillEndEvent;

        Log.Print("Player exited Skill state");
    }

    /// <summary>
    /// 입력 처리
    /// 회피 중 입력을 감지하여 다음 상태를 결정
    /// </summary>
    public void HandleInput()
    {
        if (p_context.Controller.DodgeInput && p_context.Movement.CanDodge())
        {
            _nextState = typeof(PlayerDodgeState);
        }
        else if (p_context.Controller.DefendInput)
        {
            _nextState = typeof(PlayerDefendState);
        }
        else if (p_context.Combat.CanCounterAttack && p_context.Controller.AttackInput)
        {
            _nextState = typeof(PlayerCounterAttackState);
        }
        else if (p_context.Controller.AttackHeldInput)
        {
            _nextState = typeof(PlayerMeleeAttackChargeState);
        }
        else if (p_context.Controller.AttackInput)
        {
            _nextState = typeof(PlayerFirstMeleeAttackState);
        }
        else if (p_context.Controller.RangedAttackInput)
        {
            _nextState = typeof(PlayerRangedAttackChargeState);
        }
        else if (p_context.Controller.MoveInput != Vector2.zero)
        {
            _nextState = typeof(PlayerMoveState);
        }

        if (_nextState != null)
        {
            Log.PrintColor(Color.skyBlue, $"[PlayerAttackBaseState] 다음 상태: {_nextState}");
        }
    }

    private void HandleMovement()
    {
        if (p_context.Movement != null && p_context.Controller.MoveInput != Vector2.zero)
        {
            // 2D 입력을 3D 월드 좌표로 변환
            Vector3 moveDirection = new Vector3(p_context.Controller.MoveInput.x, 0, p_context.Controller.MoveInput.y);
            p_context.Movement.Move(moveDirection, p_context.Movement.MoveSpeed / 4.0f); 
        }
    }
    
    private void SkillEndEvent(Vector3 position)
    {
         // 저장된 다음 상태로 전환
        if (_nextState != null)
        {
            p_stateMachine.ChangeState(_nextState);
        }
        else
        {
            // 아무 입력이 없었으면 Idle 상태로
            p_stateMachine.ChangeState<PlayerIdleState>();
        }
    }

}
