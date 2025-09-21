using BH_Lib.FSM;
using UnityEngine;

namespace player.Refactor
{
    public class PlayerChargeState : BaseState<Player>
    {
        private bool _isCharged = false;

        private float _lastChargeTime;
        private float _chargeGuage;
        private SourceMap chargeSourceMap;

        public PlayerChargeState(Player context, StateMachine<Player> stateMachine)
            : base(context, stateMachine) { }

        public override void OnEnter()
        {
            p_context.Animator.SetBool("IsCharge", true);
            p_context.Events.OnTier1Up += HandleMinChargeFinish;
            p_context.Heat.OnHeatChanged += HandleSetupChargeSourceMap;
            SetupChargeSourceMap();
            _chargeGuage = 0;
            _lastChargeTime = -999;

            p_context.Events.TriggerChargeStart();
        }


        public override void OnUpdate()
        {
            p_context.Movement?.Move(Vector3.zero, 0f, 0f);

            // 열기가 1만 올라가는 간격 구하기
            float tickCounter = chargeSourceMap.TickSecond / (float)chargeSourceMap.DeltaHeat;
            if (Time.time - _lastChargeTime > tickCounter)
            {
                _chargeGuage += 1;
                p_context.Heat.IncreaseHeatOnCharge(chargeSourceMap, _chargeGuage);
                _lastChargeTime = Time.time;
            }

            if (!p_context.Controller.AttackHeldInput)
            {
                if (_isCharged)
                {
                    p_stateMachine.ChangeState<PlayerChargeAttackState>();
                    return;
                }
                else
                {
                    p_stateMachine.ChangeState<PlayerIdleState>();
                    return;
                }
            }
            else if(p_context.Controller.DodgeInput)
            {
                p_stateMachine.ChangeState <PlayerDodgeState>();
            }

            // 에임 방향으로 회전
            var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
            var moveInput = p_context.Controller.MoveInput;
            var mousePosition = p_context.Controller.MousePosition;
            p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);
        }



        public override void OnExit()
        {
            p_context.Animator.SetBool("IsCharge", false);

            p_context.Events.OnTier1Up -= HandleMinChargeFinish;
            p_context.Heat.OnHeatChanged -= HandleSetupChargeSourceMap;
        }

        /// <summary>
        /// 최소 차지가 완료되었을 때 발생하는 이벤트
        /// </summary>
        private void HandleMinChargeFinish()
        {
            if(!_isCharged)
            {
                _isCharged = true;
                p_context.Events.TriggerChargeFinish();
            }
        }
        /// <summary>
        /// 열기 티어가 바뀔 때마다 차징 소스맵 변경
        /// </summary>
        /// <param name="previousHeat">이전 열기</param>
        /// <param name="currentHeat">현재 열기</param>
        private void HandleSetupChargeSourceMap(int previousHeat, int currentHeat)
        {
            SetupChargeSourceMap();
        }
        /// <summary>
        /// 차지 소스맵 등록
        /// </summary>
        private void SetupChargeSourceMap()
        {
            chargeSourceMap = p_context.DataBase.SourceMapData.
                GetSourceMap("OnCharge", p_context.Heat.CurrentTier);
        }
    }

}
