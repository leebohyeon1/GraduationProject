using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using UnityEngine;

namespace player.Refactor
{
    /// <summary>
    /// 플레이어 대기 상태
    /// 입력이 없을 때의 기본 상태
    /// </summary>
    public class PlayerIdleState : BaseState<Player>
    {
        public PlayerIdleState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

        public override void OnEnter()
        {
            p_context.Animator.SetBool("IsIdle", true);
            
            // 대기 상태 진입 시 처리
            Log.Print("Player entered Idle state");
        }

        public override void OnUpdate()
        {
            if(Time.time - p_context.Combat.LastBattleTime >= p_context.DataBase.RuntimeData.BattleOutTime 
                && p_context.Combat.IsBattleState)
            {
                p_context.Events.TriggerBattleStateChanged(false);
            }
        }

        public override void OnFixedUpdate()
        {
            // Idle 상태에서도 중력 적용 (이동 입력 없이)
            p_context.Movement?.Move(Vector3.zero, 0f, 0f);
        }

        public override void OnExit()
        {
            p_context.Animator.SetBool("IsIdle", false);
            Log.Print("Player exited Idle state");
        }
    }
}

