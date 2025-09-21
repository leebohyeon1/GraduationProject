using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

namespace player.Refactor
{
    public class PlayerHitState : BaseState<Player>
    {
        private float _hitDuration = 0.1f; // 피격 상태 지속 시간
        private float _hitTimer;

        public PlayerHitState(Player context, StateMachine<Player> stateMachine) 
            : base(context, stateMachine) 
        {
            _hitDuration = p_context.DataBase.RuntimeData.HitStunDuration;
        }

        public override void OnEnter()
        {
            if (p_context.Health.IsDefending)
            {
                p_context.Animator.SetTrigger("DefendHit");
            }
            else
            {
                p_context.Animator.SetTrigger("Hit");
            }

            p_context.Animator.SetBool("IsHit", true);

            _hitTimer = 0f;

            // 피격 시 이동 정지
            p_context.Movement?.Move(Vector3.zero, 0f, 0f);

            Log.Print("Player entered Hit state");
        }


        public override void OnUpdate()
        {
            _hitTimer += Time.deltaTime;

            // 피격 상태에서도 중력 적용
            p_context.Movement?.Move(Vector3.zero, 0f, 0f);

            // 피격 지속 시간이 끝나면 Idle 상태로 전환
            if (_hitTimer >= _hitDuration)
            {
                if (p_context.Health.IsDefending)
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

            p_context.Health.ResetHitState();
            Log.Print("Player exited Hit state");
        }
    }
}
