using System;
using BH_Lib.FSM;
using UnityEngine;
using UnityEngine.UIElements;

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

    private readonly Action<Vector3> _attackFinishedHandler;

    /// <summary>
    /// 두 번째 공격 상태 생성자
    /// </summary>
    public PlayerSecondMeleeAttackState(PlayerContext context, StateMachine<PlayerContext> stateMachine)
        : base(context, stateMachine)
    {
        _attackFinishedHandler = (Position) => AttackFinished();
    }

    public override void OnEnter()
    {
        base.OnEnter();

        p_context.Event.MeleeAttack.OnFinished += _attackFinishedHandler;
        p_context.Event.MeleeAttack.PublishStart(p_context.MeleeAttack.AttackStartEffectPosition);
        p_context.Event.PublishMeleeAttackEffect(p_context.MeleeAttack.ComboCount, p_context.MeleeAttack.AttackStartEffectPosition);

    }

    public override void OnExit()
    {
        base.OnExit();

        p_context.Event.MeleeAttack.OnFinished -= _attackFinishedHandler;
    }
}