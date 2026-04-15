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
        p_owner.Combat.ResetHeavyAttackComboIndex();       // 강공격 콤보 순서 초기화
        p_owner.Combat.SetCharge(false);                  // 차지 레벨 초기화
        p_owner.Combat.TriggerBattleStateChanged(true);     // 전투 상태 유지

        // 기절 중 스테미나 회복 중지
        p_owner.Events.TriggerRegenStamina(false);
        
        _knockbackTimer = 0f;   // 타이머 초기화

        KnockbackMovement();

        p_owner.AnimationTrigger.PlayFeedback("Player_KnockDown_Damage_FB");
    }

    /// <summary>
    /// 넉백 움직임
    /// </summary>
    private void KnockbackMovement()
    {
        // 항상 플레이어의 뒤쪽 방향으로 설정
        Vector3 moveDirection = -p_owner.transform.forward;
        StepData knockbackData = new StepData
        {
            StepCurve = p_owner.Data.KnockdownStepCurve,
            StepDuration = p_owner.Data.KnockdownStepDuration,
            StepDistance = p_owner.Data.KnockdownStepDistance,
            StepRotateSpeed = 0f
        };

        // Step을 사용하여 뒤로 밀려남
        p_owner.Movement.Step(moveDirection, knockbackData, this, true);
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
        base.ClearStats();

        // 기절 상태 종료 시 스테미나 회복 재개
        p_owner.Events.TriggerRegenStamina(true);

        DOTween.Kill(this);
        p_owner.Combat.TriggerBattleStateChanged(true);
    }

    protected override void ClearAnimator()
    {
        base.ClearAnimator();
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