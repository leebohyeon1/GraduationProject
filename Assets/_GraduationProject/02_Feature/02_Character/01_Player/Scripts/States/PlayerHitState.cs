using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;


public class PlayerHitState : BaseState<Player>
{
    private float _hitDuration = 0.1f; // 피격 상태 지속 시간
    private float _hitTimer;

    public PlayerHitState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        if (p_context.RuntimeData.IsHeavyHit)
        {
            p_context.Events.TriggerTakeDamge(PlayerDamagedType.Strong);
        }
        else if(p_context.RuntimeData.IsLightHit)
        {
            if (p_context.RuntimeData.IsDefending)
            {
                p_context.Animator.SetTrigger("DefendHit");
                p_context.Events.TriggerTakeDamge(PlayerDamagedType.Defend);
            }
            else
            {
                p_context.Animator.SetTrigger("Hit");
                p_context.Events.TriggerTakeDamge(PlayerDamagedType.Normal);
            }

        }

        p_context.Animator.SetBool("IsHit", true);

        _hitDuration = p_context.Health.StiffnessDuration;
        _hitTimer = 0f;


        // 피격 시 이동 정지
        p_context.Movement?.Move(Vector3.zero, 0f, 0f);
        p_context.Events.TriggerBattleStateChanged(true);
    }


    public override void OnUpdate()
    {
        _hitTimer += Time.deltaTime;

        // 피격 상태에서도 중력 적용
        p_context.Movement?.Move(Vector3.zero, 0f, 0f);

        // 피격 지속 시간이 끝나면 Idle 상태로 전환
        if (_hitTimer >= _hitDuration)
        {
            if (p_context.RuntimeData.IsDefending)
            {
                p_stateMachine.RevertToPreviousState();
            }
            else
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
            }

        }
    }

    public override void OnExit()
    {
        p_context.Animator.SetBool("IsHit", false);

        p_context.RuntimeData.ResetDamaged();
        p_context.Events.TriggerBattleStateChanged(true);
    }
}

