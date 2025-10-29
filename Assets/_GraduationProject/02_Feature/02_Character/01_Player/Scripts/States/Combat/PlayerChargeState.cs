using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어의 차지 상태입니다.
/// </summary>
public class PlayerChargeState : BaseState<Player>
{
    private bool _isCharged = false; // 최소 차지 완료 여부

    private float _chargeTimer; // 차지 시간 타이머
    private float _chargeGuage; // 현재 차지 게이지
    private SourceMap _chargeSourceMap; // 차지 관련 소스맵 데이터

    public PlayerChargeState(Player context, StateMachine<Player> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Heat.OnTierChanged += HandleTierChanged;

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

        // 일정 시간마다 차지 게이지 및 열기 증가
        if (_chargeTimer > _chargeSourceMap.TickSecond)
        {
            float previousGuage = _chargeGuage;

            _chargeTimer = 0;
            _chargeGuage += (int)_chargeSourceMap.HeatChangeType * _chargeSourceMap.DeltaHeat;

            float currentGuage = Mathf.Clamp(_chargeGuage, 0f, 100f);

            if(previousGuage != currentGuage)
            {
                p_context.Events.TriggerBattleStateChanged(true);
                p_context.Heat.IncreaseHeatOnCharge(_chargeSourceMap, _chargeGuage);

                if (currentGuage >= 25 && !_isCharged)
                {
                    Log.Print("Charge Minimum Complete");
                    _isCharged = true;
                }

                if (Mathf.FloorToInt(previousGuage / 25f) != Mathf.FloorToInt(currentGuage / 25f))
                {
                    
                    int tier = Mathf.FloorToInt(currentGuage / 25f);
                    Log.Print("Charge Tier Up: " + tier);
                    p_context.Events.TriggerChargeFinish(tier);
                }
            }

           
        }

        // 입력에 따른 상태 전환
        if (!p_context.Input.AttackHeldInput)
        {
            if (_isCharged && !p_context.Heat.IsOverHeat)
            {
                int tier = Mathf.FloorToInt(_chargeGuage / 25f) == 4 ? 3 : Mathf.FloorToInt(_chargeGuage / 25f);
                p_context.Events.TriggerChargeAttackStart(tier);

                p_context.Heat.ChangeHeat(-Mathf.FloorToInt(_chargeGuage));
                p_stateMachine.ChangeState<PlayerChargeAttackState>();   
            }
            else
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
            }
        }
        else if(p_context.Input.DodgeInput && Time.time - p_context.Movement.LastDodgeTime >= p_context.Stats.BasePlayerDatasSO.CombatData.DodgeCooldown)
        {
            p_stateMachine.ChangeState <PlayerDodgeState>();
        }

        // 조준 방향으로 회전
        var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
        var moveInput = p_context.Input.MoveInput;
        var mousePosition = p_context.Input.MousePosition;
        p_context.Movement.RotateToDirection(deviceType, moveInput, mousePosition);
    }

    public override void OnExit()
    {
        p_context.Heat.OnTierChanged -= HandleTierChanged;
                
        p_context.Heat.TriggerChargeGuageChanged(0f);
        p_context.Animator.SetBool("IsCharge", false);
        p_context.Events.TriggerBattleStateChanged(true);
        p_context.Events.TriggerChargeCancel();
    }

    /// <summary>
    /// 열기 티어 변경 시 차지 소스맵을 다시 설정합니다.
    /// </summary>
    private void HandleTierChanged(int previousTier, int currentTier)
    {
        SetupChargeSourceMap();
    }

    /// <summary>
    /// 현재 티어에 맞는 차지 소스맵을 설정합니다.
    /// </summary>
    private void SetupChargeSourceMap()
    {
        _chargeSourceMap = p_context.DataBase.SourceMapData.GetSourceMap("OnCharge", p_context.Heat.CurrentTier);
    }

}