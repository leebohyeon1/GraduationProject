using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;

/// <summary>
/// 플레이어의 원거리 공격 상태입니다.
/// </summary>
public class PlayerRangedAttackState : BaseState<Player>
{
    private Type _nextState; // 다음 전환될 상태
    private RangedAttackData _attackData => p_context.Stats.CombatData.RangedAttackData; // 원거리 공격 데이터

    public PlayerRangedAttackState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnRangedAttackFinish += HandleAttackFinish;
        p_context.Animator.SetTrigger("RangedAttack");

        p_context.Events.TriggerRangedAttackStart();
        p_context.Events.TriggerBattleStateChanged(true);
    }

    public override void OnUpdate() 
    {
        HandleInput();
    }

    public override void OnExit()
    {
        p_context.Events.OnRangedAttackFinish -= HandleAttackFinish;
        p_context.Events.TriggerBattleStateChanged(true);
    }

    /// <summary>
    /// 공격 중 입력을 처리하여 다음 상태를 결정합니다.
    /// </summary>
    public void HandleInput()
    {
        if (p_context.Input.AttackInput)
        {
            var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
            var moveInput = p_context.Input.MoveInput;
            var mousePosition = p_context.Input.MousePosition;
            p_context.Movement.SetTargetRotation(p_context.Movement.GetTargetRotation(deviceType, moveInput, mousePosition));
            _nextState = typeof(PlayerFirstAttackState);
        }
        else if (p_context.Input.DodgeInput && Time.time - p_context.Movement.LastDodgeTime >= p_context.Stats.CombatData.DodgeCooldown)
        {
            _nextState = typeof(PlayerDodgeState);
        }
        else if (p_context.Input.DefendInput)
        {
            _nextState = typeof(PlayerDefendState);
        }
        else if (p_context.Input.AttackHeldInput)
        {
            _nextState = typeof(PlayerChargeState);
        }
        else if (p_context.Input.RangedAttackInput)
        {
            _nextState = typeof(PlayerRangedChargeState);
        }
    }

    /// <summary>
    /// 공격 애니메이션 종료 시 호출됩니다.
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