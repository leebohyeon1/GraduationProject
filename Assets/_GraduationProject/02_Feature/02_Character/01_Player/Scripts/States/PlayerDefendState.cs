using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System.Threading;
using UnityEngine;

/// <summary>
/// 플레이어의 방어 상태입니다.
/// </summary>
public class PlayerDefendState : BaseState<Player>
{
    public PlayerDefendState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Events.OnParryPerform += HandleParryPerform;
        p_context.Events.OnParryAffect += HandleParryAffect;

        var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
        var moveInput = p_context.Input.MoveInput;
        var mousePosition = p_context.Input.MousePosition;
        p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);

        p_context.Animator.SetBool("IsDefending", true);
        p_context.Combat.SetDefending(true);
        p_context.Events.TriggerBattleStateChanged(true);
    }

    public override void OnUpdate()
    {
        p_context.Movement?.Move(Vector3.zero, 0f, 0f);

        // 입력에 따른 상태 전환
        if (!p_context.Input.DefendInput)
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
            p_context.Combat.SetDefending(false);
        }
        else if (p_context.Input.DodgeInput)
        {
            p_stateMachine.ChangeState<PlayerDodgeState>();
            p_context.Combat.SetDefending(false);
        }
    }

    public override void OnExit()
    {
        p_context.Events.OnParryPerform -= HandleParryPerform;
        p_context.Events.OnParryAffect -= HandleParryAffect;

        DOTween.Kill(this);
        p_context.Animator.SetBool("IsDefending", false);
        p_context.Events.TriggerBattleStateChanged(true);
    }

    /// <summary>
    /// 패링 판정이 발생하는 시점에 호출됩니다.
    /// </summary>
    private void HandleParryPerform()
    {
        Collider[] colliders = p_context.Combat.ExecuteParry(p_context.Stats.CombatData.ParryRadius);

        foreach (Collider collider in colliders)
        {
            if(collider.TryGetComponent<IParryable>(out var parryable) && parryable.IsParryable)
            {
                parryable.Parry(p_context.gameObject);  
                p_context.Events.TriggerParryAffect(collider);
            }
        }
    }

    /// <summary>
    /// 패링 성공 시 호출됩니다.
    /// </summary>
    private void HandleParryAffect(Collider collider)
    {
        p_context.Combat.ToggleCanCounter(p_context.Stats.CombatData.CounterAttackWindow);
        KnockbackMovement(collider.transform);
    }


    private void KnockbackMovement(Transform parryObject)
    {
        Vector3 moveDirection = (p_context.transform.position - parryObject.position).normalized;

        float currentDistance = 0f;
        DOTween.To(
            () => currentDistance,
            x =>
            {
                Vector3 displacement = moveDirection * (x - currentDistance);
                p_context.Movement.ForceMove(displacement);
                currentDistance = x;
            },
            p_context.Stats.CombatData.ParryMoveForce * p_context.Stats.CombatData.ParryMoveDuration, 
            0.1f)
            .SetEase(p_context.Stats.CombatData.KnockbackCurve)
            .SetId(this)
            .SetUpdate(UpdateType.Fixed);
    }
}