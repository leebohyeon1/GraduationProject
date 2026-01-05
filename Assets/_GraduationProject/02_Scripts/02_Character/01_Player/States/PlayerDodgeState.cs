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
    : base(context, stateMachine) 
    {
        p_context.Events.DodgeFinished += OnDodgeFinished;
        p_context.Events.DodgeStarted += OnDodgeStarted;
    }

    ~PlayerDodgeState()
    {
        p_context.Events.DodgeFinished -= OnDodgeFinished;
        p_context.Events.DodgeStarted -= OnDodgeStarted;
    }

    public override void OnEnter()
    {
        _nextState = null;

        p_context.Stats.AttackComboIndex = 0;
        p_context.Animator.SetInteger("ComboIndex", p_context.Stats.AttackComboIndex);

        p_context.Animator.SetTrigger("Dodge");

        // 입력 방향에 따라 회피 방향 결정
        if (p_context.Input.MoveInput == Vector2.zero)
        {
            _dodgeDirection = p_context.transform.forward;
        }
        else
        {
            _dodgeDirection = new Vector3(p_context.Input.MoveInput.x, 0, p_context.Input.MoveInput.y).normalized;
            //p_context.Movement.RotateToDirection(_dodgeDirection);
        }

        if(p_context.Stats.IsLockOn)
        {
            p_context.Movement.RotateToDirection(_dodgeDirection);
        }
    }

    public override void OnUpdate()
    {
        HandleInput();
    }

    public override void OnExit()
    {

        p_context.Health.SetInvisible(false); // 무적 해제
        DOTween.Kill(this);

        // 전투 상태일 때 구르기만 전투 상태 유지
        if (p_context.Combat.IsBattleState)
        {
            p_context.Events.TriggerBattleStateChanged(true);
        }
    }

    public void OnDodgeStarted()
    {
        // 전투 상태일 때 구르기만 전투 상태 유지
        if (p_context.Combat.IsBattleState)
        {
            p_context.Events.TriggerBattleStateChanged(true);
        }

        // 구르기 시작
        float distance = p_context.Stats.RuntimeData.CombatData.DodgeDistance;
        float duration = p_context.Stats.RuntimeData.CombatData.DodgeDuration;
        AnimationCurve curve = p_context.Stats.RuntimeData.CombatData.DodgeAnimationCurve;

        float currentDistance = 0f;
        DOTween.To(
            () => currentDistance,
            x =>
            {
                _dodgeDirection = new Vector3(p_context.Input.MoveInput.x, 0, p_context.Input.MoveInput.y).normalized;
                float deltaDistance = x - currentDistance;

                deltaDistance *= distance / duration;
                p_context.Movement?.Dodge(_dodgeDirection, deltaDistance, p_context.Stats.RuntimeData.CombatData.DodgeRotateSpeed);
                currentDistance = x;
            },
            distance, duration).
            SetEase(curve).
            SetId(this).
            SetUpdate(UpdateType.Fixed);
    }

    /// <summary>
    /// 회피 애니메이션 종료 시 호출됩니다.
    /// </summary>
    public void OnDodgeFinished()
    {
        p_stateMachine.ChangeState<PlayerIdleState>();
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
            _nextState = typeof(PlayerAttackState);
        }
        else if(p_context.Input.DodgeInput && p_context.Stamina.CheckStamina())
        {
            _nextState = typeof(PlayerDodgeState);
        }
        else if (p_context.Input.AttackHeldInput && p_context.Stamina.CheckStamina())
        {
            _nextState = typeof(PlayerChargeState);
        }

     }
}