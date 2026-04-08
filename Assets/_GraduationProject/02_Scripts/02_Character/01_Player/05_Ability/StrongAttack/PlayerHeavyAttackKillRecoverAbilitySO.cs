using UnityEngine;

/// <summary>
/// 강공격으로 적 처치 시 카운터 스택을 회복하는 능력입니다.
/// </summary>
[CreateAssetMenu(fileName = "PlayerHeavyAttackKillRecoverAbility", menuName = "Project/Player/Ability/Ability/HeavyAttackKillRecover")]
public class PlayerHeavyAttackKillRecoverAbilitySO : PlayerAbilitySO
{
    public override void RegisterAbility(PlayerAbility ability)
    {
        base.RegisterAbility(ability);
        
        // 공격 이벤트 구독
        p_owner.Combat.AttackEvent += HandleAttack;
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        if (p_owner != null && p_owner.Combat != null)
        {
            p_owner.Combat.AttackEvent -= HandleAttack;
        }
        
        base.UnregisterAbility(ability);
    }

    private void HandleAttack(IDamageable damageable, DamageData damageData)
    {
        // 1. 적이 죽었는지 확인 (TakeDamage 이후 호출되므로 IsDead 체크 가능)
        if (!damageable.IsDead)
        {
            return;
        }

        // 2. 공격 타입이 강공격 계열인지 확인
        bool isHeavyAttack = damageData.AttackType >= AttackType.Strong_1 && damageData.AttackType <= AttackType.Strong_3;

        if (isHeavyAttack)
        {
            // 3. 조건 만족 시 스택 회복
            p_owner.Combat.AddCounterStack();
        }
    }
}
