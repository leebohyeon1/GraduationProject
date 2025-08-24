using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimationEventHandler : PlayerComponent
{
    public event Action OnAttack;         // 공격하는 시점
    public event Action OnAttackFinished; // 공격이 끝난 끝난 시점
    public event Action OnFootstep;       // 발소리가 필요한 시점


    /// <summary>
    /// 애니메이션 이벤트: 공격 시점
    /// </summary>
    public void TriggerAttack()
    {
        OnAttack?.Invoke();
    }

    /// <summary>
    /// 애니메이션 이벤트: 공격이 끝난 시점
    /// </summary>
    public void TriggerAttackFinished()
    {
        OnAttackFinished?.Invoke();
    }

    /// <summary>
    /// 애니메이션 이벤트: 발소리 시점
    /// </summary>
    public void TriggerFootstep()
    {
        OnFootstep?.Invoke();
    }
}
