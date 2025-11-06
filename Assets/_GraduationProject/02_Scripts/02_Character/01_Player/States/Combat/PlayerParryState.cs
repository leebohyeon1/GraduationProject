using BH_Lib.FSM;
using DG.Tweening;
using System;
using UnityEngine;

public class PlayerParryState : BaseState<Player>
{
    public PlayerParryState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnParryWindowFinish += HandleParryWindowFinish;
        p_context.Events.OnParryFinish += HandleParryFinish;

        p_context.Stamina.UseStamina(p_context.Stats.Data.CombatData.ParryStamina);
        p_context.Events.TriggerRegenStamina(false);


        var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
        var moveInput = p_context.Input.MoveInput;
        var mousePosition = p_context.Input.MousePosition;
        p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);

        p_context.Animator.Play("Parry",0 , 0);

        Debug.Log(11);
    }

    public override void OnExit()
    {
        p_context.Events.OnParryWindowFinish -= HandleParryWindowFinish;
        p_context.Events.OnParryFinish -= HandleParryFinish;

        p_context.Events.TriggerRegenStamina(true);

        Debug.Log(22);
    }

    private void KnockbackMovement(Transform parryObject)
    {
        Vector3 moveDirection = (p_context.transform.position - parryObject.position).normalized;

        float currentDistance =  0f;
        DOTween.To(
            () => currentDistance,
            x =>
            {
                Vector3 displacement = moveDirection * (x - currentDistance);
                p_context.Movement.ForceMove(displacement);
                currentDistance = x;
            },
            p_context.Stats.Data.CombatData.ParryMoveForce * p_context.Stats.Data.CombatData.ParryMoveDuration,
            0.1f)
            .SetEase(p_context.Stats.Data.CombatData.KnockbackCurve)
            .SetId(this)
            .SetUpdate(UpdateType.Fixed);
    }

    /// <summary>
    /// 패링 검사가 종료되는 시점
    /// </summary>
    private void HandleParryWindowFinish()
    {
        if(p_context.Stats.ParryableQueue.TryPeek(out var parryable))
        {
            for (int i = 0; i < p_context.Stats.ParryableQueue.Count; i++)
            {
                p_context.Stats.ParryableQueue.Dequeue().Parry(p_context.gameObject);
            }

            p_context.Stats.ParryableQueue.Clear();

            p_stateMachine.ChangeState<PlayerIdleState>();
        }
    }

    private void HandleParryFinish()
    {
        Debug.Log(33);
        p_stateMachine.ChangeState<PlayerIdleState>();
    }

    /// <summary>
    /// 패링 성공 시 호출됩니다.
    /// </summary>
    private void HandleParryAffect(Collider collider)
    {
        KnockbackMovement(collider.transform);
    }

}