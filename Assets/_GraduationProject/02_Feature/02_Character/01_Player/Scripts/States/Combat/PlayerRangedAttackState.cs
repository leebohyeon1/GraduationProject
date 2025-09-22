using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;

public class PlayerRangedAttackState : BaseState<Player>
{
    private Type _nextState;
    private RangedAttackData _attackData => p_context.DataBase.RuntimeData.CombatData.RangedAttackData;

    public PlayerRangedAttackState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnRangedAttackFinish += HandleAttackFinish;
        p_context.Animator.SetTrigger("RangedAttack");

        p_context.Events.TriggerRangedAttackStart();
        p_context.Events.TriggerBattleStateChanged(true);

        Log.Print("Player entered RangedAttackFireState");
    }

    public override void OnUpdate() 
    {
        HandleInput();
    }

    public override void OnExit()
    {
        p_context.Events.OnRangedAttackFinish -= HandleAttackFinish;

        p_context.Events.TriggerBattleStateChanged(true);

        Log.Print("Player exited RangedAttackFireState");
    }

    /// <summary>
    /// 입력 처리
    /// 회피 중 입력을 감지하여 다음 상태를 결정
    /// </summary>
    public void HandleInput()
    {
        if (p_context.Controller.AttackInput)
        {
            _nextState = typeof(PlayerFirstAttackState);
        }
        else if (p_context.Controller.DodgeInput &&
            Time.time - p_context.Movement.LastDodgeTime >=
            p_context.DataBase.RuntimeData.CombatData.DodgeCooldown)
        {
            _nextState = typeof(PlayerDodgeState);
        }
        else if (p_context.Controller.DefendInput)
        {
            _nextState = typeof(PlayerDefendState);
        }
        else if (p_context.Combat.CanCounterAttack && p_context.Controller.AttackInput)
        {
            //_nextState = typeof(PlayerCounterAttackState);
        }
        else if (p_context.Controller.AttackHeldInput)
        {
            _nextState = typeof(PlayerChargeState);
        }
        else if (p_context.Controller.RangedAttackInput)
        {
            _nextState = typeof(PlayerRangedChargeState);
        }
        else if (p_context.Controller.SkillInput)
        {
           // _nextState = typeof(PlayerSkillState);
        }

        if (_nextState != null)
        {
            Log.PrintColor(Color.skyBlue, $"[PlayerAttackBaseState] 다음 상태: {_nextState}");
        }
    }

    /// <summary>
    /// 공격 애니메이션 이벤트 핸들러
    /// 공격이 완료되면 다른 상태로 전환
    /// </summary>
    private void HandleAttackFinish()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.SetDelay(_attackData.AttackDelay);

        sequence.AppendCallback(() =>
        {
            if (_nextState != null)
            {
                p_stateMachine.ChangeState(_nextState);
            }
            else
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
            }
        });
    }
}


