using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationEventHandler : MonoBehaviour
{
    private PlayerEventBus _eventBus;

    public void Initialize(PlayerEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public void TriggerAllowAttackStateInput()
    {
        _eventBus.PublishAllowAttackInput();
    }

    /// <summary>
    /// 애니메이션 이벤트: 공격 시점
    /// </summary>
    public void TriggerAttack()
    {
        _eventBus.PublishAttack();
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
}
