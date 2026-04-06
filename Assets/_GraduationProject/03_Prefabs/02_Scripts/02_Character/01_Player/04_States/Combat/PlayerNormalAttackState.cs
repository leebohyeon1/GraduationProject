using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 플레이어의 첫 번째 일반 공격 상태입니다.
/// </summary>
public class PlayerNormalAttackState : PlayerAttackBaseState
{
    public PlayerNormalAttackState(StateMachine<PlayerController> stateMachine) 
        : base(stateMachine) { }

    protected override PlayerAttackConfig p_AttackConfig => p_owner.Combat.NormalAttackConfigList[p_owner.Combat.NormalAttackComboIndex];


    #region Setup Function
    protected override void SetupStats()
    {
        // 일반 공격 콤보 순서 증가
        p_owner.Combat.IncreaseNormalAttackComboIndex();

        base.SetupStats();
    }

    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        // 애니메이션 설정
        p_animator.speed += p_animator.speed * p_owner.Combat.PlusNormalAttackSpeedMultiplier;
        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.NormalAttack);
        p_animator.SetInteger("NormalAttackComboIndex", p_owner.Combat.NormalAttackComboIndex);
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

        p_animator.speed = 1;
    }
    #endregion
}