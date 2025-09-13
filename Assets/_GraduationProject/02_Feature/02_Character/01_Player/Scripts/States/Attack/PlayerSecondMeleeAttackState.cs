using System;
using BH_Lib.FSM;
using UnityEngine;

/// <summary>
/// 플레이어 두 번째 공격 상태
/// 콤보 공격의 마지막 단계로, 다음 공격 상태는 없습니다.
/// </summary>
public class PlayerSecondMeleeAttackState : PlayerMeleeAttackBaseState
{
    /// <summary>두 번째 공격 애니메이션 트리거</summary>
    protected override string p_animationTrigger => "SecondAttack";

    /// <summary>다음 공격 상태 (콤보 끝이므로 null)</summary>
    protected override Type p_nextAttackState => null;

    /// <summary>
    /// 두 번째 공격 상태 생성자
    /// </summary>
    public PlayerSecondMeleeAttackState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
        : base(context, stateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();

        p_context.Event.Player.MeleeAttack.OnFinished += AttackFinished;
        p_context.Event.Player.MeleeAttack.PublishStart();
    }

    public override void OnExit()
    {
        base.OnExit();

        p_context.Event.Player.MeleeAttack.OnFinished -= AttackFinished;
    }
}