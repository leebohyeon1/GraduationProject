using BH_Lib.FSM;
using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어 원거리 공격 차징 상태
/// 원거리 공격 키를 누르고 있을 때 활성화되며, 일정 시간 차징 후 발사할 수 있습니다.
/// 차징 중에는 이동이 불가능하고, 에임 방향으로 회전합니다.
/// </summary>
public class PlayerRangedAttackChargeState : BaseState<PlayerContext>
{
    /// <summary>현재 차징 시간</summary>
    private float _chargeTime = 0f;
    
    /// <summary>
    /// 원거리 공격 차징 상태 생성자
    /// </summary>
    public PlayerRangedAttackChargeState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        p_context.Animator.SetBool("IsRangedAttackCharging", true);
        Log.Print("Player entered RangedAttackChargeState");

        _chargeTime = 0f;
    }

    public override void OnUpdate()
    {
        // 차징 중에는 이동하지 않음 (중력만 적용)
        p_context.Movement?.Move(Vector3.zero, 0f);

        // 에임 방향으로 회전
        var deviceType = p_context.InputDeviceDetector.CurrentInputDevice;
        var lookInput = p_context.Controller.LookInput;
        var mousePosition = p_context.Controller.MousePosition;
        p_context.EventBus.PublishRotateToAttackDirection(deviceType, lookInput, mousePosition);


        // 원거리 공격 키를 떼면 Idle 상태로 전환
        if (p_context.Controller.RangedAttackInput)
        {
            _chargeTime += Time.deltaTime;
        }

        if (_chargeTime >= p_context.Stats.RangedAttackData.RangedAttackChargeTime)
        {
            Log.PrintColor(Color.brown, "차징 완료");
            // 완전 차징 완료, Fire 상태로 전환 대기
            if (!p_context.Controller.RangedAttackInput)
            {
                p_stateMachine.ChangeState<PlayerRangedAttackFireState>();
            }
        }
        else
        {
            if (!p_context.Controller.RangedAttackInput)
            {
                p_stateMachine.ChangeState<PlayerIdleState>();
            }
        }
    }

    public override void OnExit()
    {
        p_context.Animator.SetBool("IsRangedAttackCharging", false);
        Log.Print("Player exited RangedAttackChargeState");        
    }
}
