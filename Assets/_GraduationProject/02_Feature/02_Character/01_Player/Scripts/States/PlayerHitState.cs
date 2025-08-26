using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어 피격 상태
/// 데미지를 받았을 때의 상태
/// </summary>
public class PlayerHitState : BaseState<PlayerContext>
{
    private float _hitDuration = 0.1f; // 피격 상태 지속 시간
    private float _hitTimer;

    public PlayerHitState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
        : base(context, stateMachine)
    {
         _hitDuration = p_context.Stats.HitStunDuration;   
    }

    public override void OnEnter()
    {
        p_context.Animator.SetBool("IsHit", true);
        p_context.Animator.SetTrigger("Hit");
        
        _hitTimer = 0f;

        // 피격 시 이동 정지
        p_context.Movement?.Move(Vector3.zero, 0f);
        
        Log.Print("Player entered Hit state");
    }

    public override void OnUpdate()
    {
        _hitTimer += Time.deltaTime;

        // 피격 상태에서도 중력 적용
        p_context.Movement?.Move(Vector3.zero, 0f);

        // 피격 지속 시간이 끝나면 Idle 상태로 전환
        if (_hitTimer >= _hitDuration)
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
        }
    }

    public override void OnExit()
    {
        p_context.Animator.SetBool("IsHit", false);
        
        p_context.Health.ResetHitState(); // 피격 상태 플래그 리셋
        Log.Print("Player exited Hit state");
    }
}