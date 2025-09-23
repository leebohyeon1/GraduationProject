using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;


public class PlayerChargeState : BaseState<Player>
{
    private bool _isCharged = false;

    private float _chargeTimer;
    private float _chargeGuage;
    private SourceMap chargeSourceMap;

    public PlayerChargeState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Heat.OnHeatChanged += HandleSetupChargeSourceMap;

        p_context.Animator.SetBool("IsCharge", true);
        SetupChargeSourceMap();

        _isCharged = false;
        _chargeGuage = 0;
        _chargeTimer = 0;

        p_context.Events.TriggerChargeStart();
        p_context.Events.TriggerBattleStateChanged(true);
    }


    public override void OnUpdate()
    {
        p_context.Movement?.Move(Vector3.zero, 0f, 0f);

        _chargeTimer += Time.deltaTime;
        if (_chargeTimer > chargeSourceMap.TickSecond)
        {
            _chargeTimer = 0;

            _chargeGuage += (int)chargeSourceMap.HeatChangeType * chargeSourceMap.DeltaHeat;
            if(_chargeGuage >= p_context.DataBase.TierStatData.GetTierStat(1).HeatThrehold)
            {
                MinChargeFinish();
            }
               
            p_context.Heat.IncreaseHeatOnCharge(chargeSourceMap, _chargeGuage);
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
                p_context.Events.TriggerChargeCancel();
                p_stateMachine.ChangeState<PlayerIdleState>();
                return;
            }
        }
        else if(p_context.Controller.DodgeInput)
        {
            p_context.Events.TriggerChargeCancel();
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
        p_context.Heat.OnHeatChanged -= HandleSetupChargeSourceMap;
        p_context.Animator.SetBool("IsCharge", false);


        p_context.Events.TriggerBattleStateChanged(true);
    }

    /// <summary>
    /// 최소 차지가 완료 함수
    /// </summary>
    private void MinChargeFinish()
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
        Log.Print(p_context.Heat.CurrentTier);
        int tier = p_context.Heat.CurrentTier == 4 ? 3 : p_context.Heat.CurrentTier;
        chargeSourceMap = p_context.DataBase.SourceMapData.
            GetSourceMap("OnCharge", tier);
    }
}

