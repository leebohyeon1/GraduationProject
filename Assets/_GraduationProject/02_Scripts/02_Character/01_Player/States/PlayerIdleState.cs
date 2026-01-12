using DG.Tweening;
using UnityEngine;

/// <summary>
/// 플레이어의 대기 상태입니다.
/// </summary>
public class PlayerIdleState : State<Player>
{
    public PlayerIdleState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Stats.AttackComboIndex = 0;
        p_context.Animator.SetInteger("ComboIndex", p_context.Stats.AttackComboIndex);
        p_context.Animator.SetBool("IsIdle", true);
    }

    public override void OnUpdate()
    {   
        if (!p_context.Health.IsDead && p_context.Stats.IsDamaged)
        {
            p_stateMachine.ChangeState<PlayerHitState>();
        }

        if (p_context.Input.MoveInput != Vector2.zero)
        {
            p_stateMachine.ChangeState<PlayerMoveState>();
        }
        else if(p_context.Input.DodgeInput && p_context.Stamina.CheckStamina())
        {
            p_stateMachine.ChangeState<PlayerDodgeState>();
        }
        else if (p_context.Input.AttackInput && p_context.Stamina.CheckStamina())
        {
            p_stateMachine.ChangeState<PlayerAttackState>();
        }
        else if (p_context.Input.AttackHeldInput && p_context.Stamina.CheckStamina())
        {
            p_stateMachine.ChangeState<PlayerChargeState>();
        }
        else if (p_context.Input.ParryInput && p_context.Stamina.CheckStamina())
        {
            p_stateMachine.ChangeState <PlayerParryState>();
        }
    }

    public override void OnFixedUpdate()
    {
        if (p_context.Stats.IsLockOn)
        {
            Vector3 targetPosition = new Vector3(p_context.LockOnSystem.CurrentTarget.position.x, 0, p_context.LockOnSystem.CurrentTarget.position.z);
            Vector3 directionToTarget = (targetPosition - new Vector3(p_context.transform.position.x, 0, p_context.transform.position.z)).normalized;

            p_context.Movement?.SetRotation(Quaternion.LookRotation(directionToTarget), p_context.Stats.RuntimeData.RotateSpeed);
        }

        // 대기 상태에서는 움직이지 않음
        p_context.Movement?.Move(Vector3.zero, 0f);
    }

    public override void OnExit()
    {
        p_context.Animator.SetBool("IsIdle", false);
    }
}