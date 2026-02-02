using DG.Tweening;
using UnityEngine;

/// <summary>
/// 플레이어의 피격 상태입니다.
/// </summary>
public class PlayerKnockdownState : PlayerBaseState
{
    private float _knockbackTimer; // 피격 시간 타이머

    public PlayerKnockdownState(StateMachine<PlayerController> stateMachine) 
        : base(stateMachine) 
    {
        p_owner.Events.Knockdown += OnKnockdown;
    }

    ~PlayerKnockdownState()
    {
        p_owner.Events.Knockdown -= OnKnockdown;
    }

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnUpdate()
    {
        _knockbackTimer += Time.deltaTime;

        p_owner.Movement?.Move(Vector3.zero, 0f, 0f);

        // 경직 시간이 지나면 상태 전환
        if (_knockbackTimer >= p_owner.Health.KnockDownDuration)
        {
            p_stateMachine.ChangeState<PlayerIdleState>();
        }
    }

    #region Setup Function
    protected override void SetupStats()
    {
        base.SetupStats();

        p_owner.Combat.ResetNormalAttackComboIndex();       // 일반 공격 콤보 순서 초기화
        p_owner.Combat.ResetChargeLevel();                  // 차지 레벨 초기화
        p_owner.Combat.TriggerBattleStateChanged(true);     // 전투 상태 유지
        
        _knockbackTimer = 0f;   // 타이머 초기화
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.Knockdown);
    }
    #endregion

    #region Clear Function
    protected override void ClearStats()
    {
        base.SetupStats();

        DOTween.Kill(this);
        p_owner.Combat.TriggerBattleStateChanged(true);
    }

    protected override void ClearAnimator()
    {
        base.ClearAnimator();

        p_animator.SetBool("IsHit", false);
    }
    #endregion

    /// <summary>
    /// 기절 상태로 전환 이벤트
    /// </summary>
    private void OnKnockdown()
    {
        // 상태 전환
        p_stateMachine.ChangeState<PlayerKnockdownState>();
    }
}