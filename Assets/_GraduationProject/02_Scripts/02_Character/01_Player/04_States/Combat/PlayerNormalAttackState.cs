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

    protected override PlayerAttackConfig p_AttackConfig => p_owner.Data.NormalAttackConfigList[p_owner.Combat.NormalAttackComboIndex];


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
        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.NormalAttack);
        p_animator.SetInteger("NormalAttackComboIndex", p_owner.Combat.NormalAttackComboIndex);
    }
    #endregion

    #region Clear Function
    protected override void ClearStats()
    {
        base.ClearStats();

        // 일반 공격을 더이상 할 수 없을 때 초기화
        if(!p_owner.Combat.CanNormalAttack())
        {
            p_owner.Combat.ResetNormalAttackComboIndex();
        }
    }

    #endregion
}