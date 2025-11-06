using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;

/// <summary>
/// 플레이어의 회피 상태입니다.
/// </summary>
public class PlayerDodgeState : BaseState<Player>
{
    private Vector3 _dodgeDirection; // 회피 방향
    private Type _nextState; // 다음 전환될 상태

    public PlayerDodgeState(Player context, StateMachine<Player> stateMachine)
    : base(context, stateMachine) { }

    public override void OnEnter()
    {
        _nextState = null;

        p_context.Animator.SetTrigger("Dodge");
        p_context.Events.OnDodgeFinish += HandleDodgeFinish;

        // 입력 방향에 따라 회피 방향 결정
        if (p_context.Input.MoveInput == Vector2.zero)
        {
            _dodgeDirection = p_context.transform.forward;
        }
        else
        {
            _dodgeDirection = new Vector3(p_context.Input.MoveInput.x, 0, p_context.Input.MoveInput.y).normalized;
            p_context.Movement.RotateToDirection(_dodgeDirection);
        }


        p_context.Health.SetInvisible(true); // 회피 중 무적

        if (p_context.Combat.IsBattleState)
        {
            p_context.Events.TriggerBattleStateChanged(true);
        }

        p_context.Events.TriggerDodgeStart();
        p_context.Stamina.UseStamina(p_context.Stats.Data.CombatData.DodgeStamina);

        float distance = p_context.Stats.Data.CombatData.DodgeDistance;
        float duration = p_context.Stats.Data.CombatData.DodgeDuration;
        AnimationCurve curve = p_context.Stats.Data.CombatData.DodgeAnimationCurve;

        float currentDistance = 0f;
        DOTween.To(
            () => currentDistance,
            x =>
            {
                _dodgeDirection = new Vector3(p_context.Input.MoveInput.x, 0, p_context.Input.MoveInput.y).normalized;
                p_context.Movement?.Dodge(_dodgeDirection, distance/duration, p_context.Stats.Data.CombatData.DodgeRotateSpeed);

                currentDistance = x;
            },
            distance, duration).
            SetEase(curve).
            SetId(this).
            SetUpdate(UpdateType.Fixed);
    }

    public override void OnUpdate()
    {
        HandleInput();
    }

    public override void OnExit()
    {
        p_context.Events.OnDodgeFinish -= HandleDodgeFinish;
        p_context.Health.SetInvisible(false); // 무적 해제
        DOTween.Kill(this);


        if (p_context.Combat.IsBattleState)
        {
            p_context.Events.TriggerBattleStateChanged(true);
        }
    }

    /// <summary>
    /// 회피 애니메이션 종료 시 호출됩니다.
    /// </summary>
    public void HandleDodgeFinish()
    {
        if (_nextState != null)
        {
            p_stateMachine.ChangeState(_nextState);
        }
        else
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
        }
    }

    /// <summary>
    /// 회피 중 입력을 처리하여 다음 상태를 결정합니다.
    /// </summary>
    public void HandleInput()
    {
        if (_nextState != null)
        {
            return;
        }

        else if (p_context.Input.AttackInput && p_context.Stamina.CheckStamina())
        {
            var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
            var moveInput = p_context.Input.MoveInput;
            var mousePosition = p_context.Input.MousePosition;
            p_context.Movement.SetTargetRotation(p_context.Movement.GetTargetRotation(deviceType, moveInput, mousePosition));
            _nextState = typeof(PlayerFirstAttackState);
        }
        else if(p_context.Input.DodgeInput && p_context.Stamina.CheckStamina())
        {
            _nextState = typeof(PlayerDodgeState);
        }
        else if(p_context.Input.ParryInput && p_context.Stamina.CheckStamina())
        {
            _nextState = typeof(PlayerParryState);
        }
    }
}