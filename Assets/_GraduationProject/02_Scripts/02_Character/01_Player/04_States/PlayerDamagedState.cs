using UnityEngine;

/// <summary>
/// 플레이어가 데미지를 받아 
/// 조금의 경직이라도 걸렸을 때 상태 
/// </summary>
public class PlayerDamagedState : PlayerBaseState
{
    private DamageData _damageData;

    public PlayerDamagedState(StateMachine<PlayerController> stateMachine) 
        : base(stateMachine) 
    {
        p_owner.Events.Damaged += OnDamaged;
    }

    ~PlayerDamagedState()
    {
        p_owner.Events.Damaged -= OnDamaged; 
    }

    #region Setup Function
    protected override void SetupStats()
    {
        base.SetupStats();

        p_owner.Combat.ResetNormalAttackComboIndex();       // 일반 공격 콤보 순서 초기화
        p_owner.Combat.ResetHeavyAttackComboIndex();       // 강공격 콤보 순서 초기화
        p_owner.Combat.SetCharge(false);                  // 차지 레벨 초기화
        p_owner.Combat.TriggerBattleStateChanged(true);     // 전투 상태 유지

        // 피격 중 스테미나 회복 중지
        p_owner.Events.TriggerRegenStamina(false);

        KnockbackMovement();

        p_owner.AnimationTrigger.PlayFeedback("Player_Normal_Damage_FB");
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.Damaged);
    }
    #endregion

    #region Clear Function
    protected override void ClearStats()
    {
        base.ClearStats();

        // 피격 상태 종료 시 스테미나 회복 재개
        p_owner.Events.TriggerRegenStamina(true);

        p_owner.Combat.TriggerBattleStateChanged(true);
        _damageData = default;
    }
    #endregion

    /// <summary>
    /// 넉백 움직임
    /// </summary>
    private void KnockbackMovement()
    {
        Vector3 moveDirection = (p_owner.transform.position - _damageData.AttackerTransform.position).normalized;

        StepData knockbackData = new StepData
        {
            StepCurve = _damageData.KnockbackCurve,
            StepDuration = _damageData.KnockbackDuration,
            StepDistance = _damageData.KnockbackForce,
            StepRotateSpeed = 0f
        };

        p_owner.Movement.Step(moveDirection, knockbackData, this, true, () => p_stateMachine.ChangeState<PlayerIdleState>());
    }

    /// <summary>
    /// 데미지 받는 상태로 전환 이벤트
    /// </summary>
    /// <param name="damageData">받은 데미지 데이터</param>
    private void OnDamaged(DamageData damageData)
    {
        // Knockdown 상태이면 반환
        if(p_stateMachine.CurrentState.GetType() == typeof(PlayerKnockdownState))
        {
            return;
        }

        // 상태가 바뀌기 전에 데미지 데이터 초기화
        _damageData = damageData;

        // 상태 전환
        p_stateMachine.ChangeState<PlayerDamagedState>();
    }
}
