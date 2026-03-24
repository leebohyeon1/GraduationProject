using UnityEngine;

/// <summary>
/// 플레이어의 강공격 상태입니다. (패리 스택 소모)
/// </summary>
public class PlayerHeavyAttackState : PlayerAttackBaseState
{
    public PlayerHeavyAttackState(StateMachine<PlayerController> stateMachine) 
        : base(stateMachine) { }

    protected override PlayerAttackConfig p_AttackConfig => p_owner.Combat.HeavyAttackConfigList[p_owner.Combat.HeavyAttackComboIndex % p_owner.Combat.HeavyAttackConfigList.Count];

    #region Setup Function
    protected override void SetupStats()
    {
        // 일반 공격 콤보 리셋 (강공격-일반 공격 연계 차단)
        p_owner.Combat.ResetNormalAttackComboIndex();

        // 강공격 콤보 순서 증가 및 스택 소모
        p_owner.Combat.IncreaseHeavyAttackComboIndex();
        p_owner.Combat.ConsumeParryStack();

        base.SetupStats();
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        // 애니메이션 설정 (짝수면 0, 홀수면 1번 애니메이션 재생)
        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.HeavyAttack); 
        p_animator.SetInteger("NormalAttackComboIndex", p_owner.Combat.HeavyAttackComboIndex % 2);
    }
    #endregion

    #region Clear Function
    protected override void ClearStats()
    {
        base.ClearStats();
    }

    protected override void ClearAnimator()
    {
        base.ClearAnimator();
    }

    public override void OnExit()
    {
        // 다음 상태가 강공격 상태가 아닐 때만 콤보 인덱스 리셋
        if (p_nextState != typeof(PlayerHeavyAttackState))
        {
            p_owner.Combat.ResetHeavyAttackComboIndex();
        }

        base.OnExit();
    }
    #endregion

    #region Input
    /// <summary>
    /// 강공격 중 일반 공격 상태 전환 허용 (콤보는 리셋됨)
    /// </summary>
    protected override void OnNormalAttack()
    {
        base.OnNormalAttack();
    }

    /// <summary>
    /// 연속 강공격 입력 처리
    /// </summary>
    protected override void OnHeavyAttack()
    {
        // 패리 스택이 남아있고 다음 콤보가 가능하면 강공격 지속
        if (p_owner.Combat.ParryStacks > 0 && p_owner.Combat.CanHeavyAttack())
        {
            if (p_nextState != null) return;
            if (!p_owner.Stamina.CheckStamina()) return;

            if (p_canChangeCombatState)
            {
                p_stateMachine.ChangeState<PlayerHeavyAttackState>();
            }
            else if (p_canBufferInput)
            {
                p_nextState = typeof(PlayerHeavyAttackState);
            }
        }
        else
        {
            // 스택이 없거나 콤보가 끝났으면 일반 공격으로 대체
            OnNormalAttack();
        }
    }
    #endregion

    #region EventHandle
    protected override void OnAttackPerformed()
    {
        // 강공격 전용 데미지 계산 로직 사용
        Collider[] hitEnemies = p_owner.Combat.ExecuteAttackWithCustomDamage(p_AttackConfig, (baseDmg) => {
            return p_owner.Combat.CalculateHeavyAttackDamage(baseDmg);
        });
    }
    #endregion
}
