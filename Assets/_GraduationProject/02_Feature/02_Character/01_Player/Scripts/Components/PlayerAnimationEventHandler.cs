using BH_Lib.Log;
using UnityEngine;

/// <summary>
/// 플레이어 애니메이션 이벤트 핸들러 클래스
/// Animator의 애니메이션 이벤트를 받아서 PlayerEventBus로 전달하는 역할을 담당합니다.
/// Unity의 Animation Event 시스템과 게임 로직을 연결하는 브릿지 역할을 합니다.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimationEventHandler : MonoBehaviour
{
    /// <summary>플레이어 이벤트 버스 참조</summary>
    private PlayerEventBus _eventBus;

    /// <summary>
    /// 애니메이션 이벤트 핸들러 초기화
    /// </summary>
    /// <param name="eventBus">플레이어 이벤트 버스</param>
    public void Initialize(PlayerEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    /// <summary>
    /// 애니메이션 이벤트: 공격 입력 허용 시점
    /// 공격 애니메이션 중 콤보 입력을 받을 수 있는 타이밍을 알려줍니다.
    /// </summary>
    public void TriggerAllowAttackStateInput()
    {
        _eventBus.PublishAllowAttackInput();
    }

    /// <summary>
    /// 애니메이션 이벤트: 공격 시점
    /// </summary>
    public void TriggerAttack()
    {
        _eventBus.PublishAttackStart();
    }

    /// <summary>
    /// 애니메이션 이벤트: 공격이 끝난 시점
    /// </summary>
    public void TriggerAttackFinished()
    {
        _eventBus.PublishAttackFinished();
    }

    /// <summary>
    /// 애니메이션 이벤트: 발소리 시점
    /// </summary>
    public void TriggerFootstep()
    {
        _eventBus.PublishFootstep();
    }

    /// <summary>
    /// 애니메이션 이벤트: 회피 애니메이션 종료 시점
    /// </summary>
    public void TriggerDodgeEnd()
    {
        _eventBus.PublishDodgeEnd();
    }

    /// <summary>
    /// 애니메이션 이벤트: 패링 시점
    /// </summary>
    public void TriggerParry()
    {
        _eventBus.PublishParry();
    }

    /// <summary>
    /// 애니메이션 이벤트: 원거리 공격 종료 시점
    /// </summary>
    public void TriggerRangedAttackEnd()
    {
        _eventBus.PublishRangedAttackEnd();
    }

    /// <summary>
    /// 애니메이션 이벤트: 근거리 차징 공격 시작 시점
    /// </summary>
    public void TriggerChargingMeleeAttack()
    {
        _eventBus.PublishChargeAttack();
    }
}
