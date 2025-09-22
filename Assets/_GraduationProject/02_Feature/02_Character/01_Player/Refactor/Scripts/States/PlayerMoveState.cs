using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

namespace player.Refactor
{
    /// <summary>
    /// 플레이어 이동 상태
    /// 이동 입력이 있을 때 활성화되는 상태
    /// </summary>
    public class PlayerMoveState : BaseState<Player>
    {
        public PlayerMoveState(Player context, StateMachine<Player> stateMachine)
            : base(context, stateMachine) { }

        public override void OnEnter()
        {
            p_context.Animator.SetBool("IsMoving", true);

            Log.Print("Player entered Move state");
        }

        public override void OnUpdate()
        {
            if (Time.time - p_context.Combat.LastBattleTime >= p_context.DataBase.RuntimeData.BattleOutTime 
             && p_context.Combat.IsBattleState)
            {
                p_context.Events.TriggerBattleStateChanged(false);
            }
        }

        public override void OnFixedUpdate()
        {
            // 이동 처리 (상태 전환은 StateMachine의 조건부 전환으로 자동 처리됨)
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (p_context.Movement != null && p_context.Controller.MoveInput != Vector2.zero)
            {
                // 2D 입력을 3D 월드 좌표로 변환
                Vector3 moveDirection = new Vector3(p_context.Controller.MoveInput.x, 0, p_context.Controller.MoveInput.y);
                p_context.Movement.Move(moveDirection, p_context.DataBase.RuntimeData.MoveSpeed, p_context.DataBase.RuntimeData.RotateSpeed);
            }
        }

        public override void OnExit()
        {
            p_context.Animator.SetBool("IsMoving", false);

            Log.Print("Player exited Move state");
        }
    }
}