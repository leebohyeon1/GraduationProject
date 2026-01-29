using UnityEngine;

/// <summary>
/// 플레이어 특수 공격 상태
/// </summary>
public class PlayerSpecialAttackState : PlayerAttackBaseState
{
    protected override PlayerAttackConfig p_AttackConfig => p_owner.Combat.SpecialAttackSO.AttackConfig;

    private CanSpecialAttackSO _canSpecialAttackSO => p_owner.Combat.SpecialAttackSO;

    public PlayerSpecialAttackState(StateMachine<PlayerController> stateMachine) 
        : base(stateMachine) { }


    protected override void SetupAnimator()
    {
        base.SetupAnimator();

        p_animator.SetInteger(p_stateParamter, (int)AnimatorState.SpecialAttack);
        p_animator.SetTrigger(_canSpecialAttackSO.AnimationTigger);
    }

}
