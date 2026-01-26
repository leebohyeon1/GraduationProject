using UnityEngine;

public class PlayerAnimationTrigger : FeedbackPlayer<string>
{
    private PlayerController p_owner;

    /// <summary>
    /// 컴포넌트 초기화 함수
    /// </summary>
    /// <param name="player">플레이어</param>
    public void Initialize(PlayerController player)
    {
        p_owner = player;
    }

    #region Input
    /// <summary>
    /// 선입력 시작 함수
    /// </summary>
    public void BufferInputStart()
    {
        p_owner.Events.TriggerBufferInputStarted();
    }
    /// <summary>
    /// 선입력 종료 함수
    /// </summary>
    public void BufferInputEnd()
    {
        p_owner.Events.TriggerBufferInputEnded();
    }
    #endregion

    #region Dodge
    /// <summary>
    /// 회피 시작 함수
    /// </summary>
    public void DodgeStart()
    {
       p_owner.Events.TriggerDodgeStarted();
    }

    #endregion

    #region Attack
    /// <summary>
    /// 공격 시작
    /// </summary>
    public void AttackStart()
    {
        p_owner.Events.TriggerAttackStarted();
    }

    /// <summary>
    /// 공격 타격
    /// </summary>
    public void AttackPerform()
    {
        p_owner.Events.TriggerAttackPerformed();
    }

    /// <summary>
    /// 공격 종료
    /// </summary>
    public void AttackEnd()
    {
        p_owner.Events.TriggerAttackFinished();
    }
    #endregion

    #region Parry
    /// <summary>
    /// 상쇄 가능 상태 시작
    /// </summary>
    public void EnableParryWindow()
    {
       // p_owner.Stats.SetParryable(true);
    }

    /// <summary>
    /// 상쇄 가능 상태 종료
    /// </summary>
    public void DisableParryWindow()
    {
       // p_owner.Stats.SetParryable(false);
    }
    #endregion
}
