using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 피격 상태입니다.
/// </summary>
public class PlayerHitState : BaseState<Player>
{
    private float _hitDuration = 0.1f; // 피격 경직 시간
    private float _hitTimer; // 피격 시간 타이머

    public PlayerHitState(Player context, StateMachine<Player> stateMachine) 
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        // 피격 종류에 따라 다른 애니메이션 및 효과 재생
        if (p_context.Stats.IsHeavyHit)
        {
            p_context.Events.TriggerTakeDamge(PlayerDamagedType.Strong);
        }
        else if(p_context.Stats.IsLightHit)
        {
            if (p_context.Stats.IsDefending)
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

        p_context.Movement?.Move(Vector3.zero, 0f, 0f); // 피격 중 이동 정지
        p_context.Events.TriggerBattleStateChanged(true);
    }

    public override void OnUpdate()
    {
        _hitTimer += Time.deltaTime;

        p_context.Movement?.Move(Vector3.zero, 0f, 0f);

        // 경직 시간이 지나면 상태 전환
        if (_hitTimer >= _hitDuration)
        {
            if (p_context.Stats.IsDefending)
            {
                p_stateMachine.RevertToPreviousState(); // 방어 중이었으면 이전 상태로 복귀
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
        p_context.Stats.ResetDamaged();
        p_context.Events.TriggerBattleStateChanged(true);
    }
}