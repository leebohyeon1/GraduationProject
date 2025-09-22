using BH_Lib.FSM;
using BH_Lib.Log;
using DG.Tweening;
using System;
using UnityEngine;

namespace player.Refactor
{
    public class PlayerChargeAttackState : PlayerAttackBaseState
    {
        protected override string p_animationTrigger => "ChargeAttack";

        protected override Type p_nextAttackState => null;

        protected override PlayerAttackData p_AttackData => p_context.DataBase.RuntimeData.CombatData.ChargeAttackData;

        private PlayerAttackData _playerAttackData;

        public PlayerChargeAttackState(Player context, StateMachine<Player> stateMachine)
            : base(context, stateMachine) { }

        public override void OnEnter()
        {
            p_context.Events.OnChargeAttackFinish += HandleAttackFinish;
            p_context.Events.OnChargeAttackPerform += HandleAttackPerform;

            p_nextState = null; // 다음 상태 초기화

            Log.Print("Player entered ChargeAttack state");
            p_context.Animator.SetTrigger(p_animationTrigger);  // 공격 애니메이션 실행
            p_context.Combat.SetupCombatCenter();
            _playerAttackData = new PlayerAttackData(p_AttackData);

            // 공격 실행
            var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
            var moveInput = p_context.Controller.MoveInput;
            var mousePosition = p_context.Controller.MousePosition;
            p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);


            // 공격 시 전진 이동 실행
            StartAttackMovement();
        }

        public override void OnExit()
        {
            p_context.Events.OnChargeAttackFinish -= HandleAttackFinish;
            p_context.Events.OnChargeAttackPerform -= HandleAttackPerform;

            p_context.Animator.ResetTrigger(p_animationTrigger);

            DOTween.Kill(p_animationTrigger);

            p_nextState = null;
            Log.Print("Player exited ChargeAttack state");
        }

        /// <summary>
        /// 공격 애니메이션 이벤트 핸들러
        /// 공격이 완료되면 다른 상태로 전환
        /// </summary>
        protected override void HandleAttackFinish()
        {
            Sequence sequence = DOTween.Sequence();
            sequence.SetDelay(p_AttackData.AttackDelay);
            sequence.AppendCallback(() =>
            {
                if (p_nextState != null)
                {
                    p_stateMachine.ChangeState(p_nextState);
                }
                else
                {
                    p_stateMachine.ChangeState<PlayerIdleState>();
                }
            });
        }

        /// <summary>
        /// 공격 효과 적용
        /// </summary>
        protected override void HandleAttackPerform()
        {
            Collider[] colliders = p_context.Combat.ExecuteAttack(_playerAttackData);

            foreach (Collider collider in colliders)
            {
                p_context.Events.TriggerChargeAttackAffect(collider, p_context.Heat.CurrentTier);
            }

            p_context.Heat.SetHeat(0);
        }

        /// <summary>
        /// 공격 시 전진 이동 시작
        /// </summary>
        protected override void StartAttackMovement()
        {
            float distance = p_AttackData.AttackMoveDistance;

            // 전방에 오브젝트가 있을 경우 전진 거리 조정
            if (Physics.Raycast(p_context.transform.position, p_context.transform.forward,
                out var hitInfo, p_AttackData.AttackMoveDistance))
            {
                distance = hitInfo.distance - (p_context.GetComponent<Collider>().bounds.size.z / 2);

                _playerAttackData.AttackRadius.z = distance + 1;
            }

            Vector3 targetPosition = p_context.transform.position + (p_context.transform.forward * distance);

            p_context.transform.DOMove(targetPosition, p_AttackData.AttackMoveDuration, false)
            .SetEase(p_AttackData.AttackMoveCurve).SetId(p_animationTrigger);
        }

    }
}
