using UnityEngine;

/// <summary>
/// 인파이팅 2티어
/// 상쇄 직후 일반 공격 시 
/// 특수 일반 공격
/// </summary>
[CreateAssetMenu(fileName = "SmashSO", menuName = "Project/Player/Ability/TheDeullist/Tier2/SmashSO")]
public class SmashSO : PlayerAbilitySO
{
    public CanSpecialAttackSO SpecialAttackSO;

    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();

        p_owner.Events.CounterSucceeded += OnCounterSucceeded;
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_owner.Events.CounterSucceeded -= OnCounterSucceeded;

        p_ability = null;
        p_owner = null;
    }

    private void OnCounterSucceeded(Transform transform)
    {
        if(!p_ability.HasTag(SpecialAttackSO))
        {
            p_ability.AddTag(SpecialAttackSO);
        }
    }

    public void Smash()
    {
        p_owner.Events.AttackFinished += OnAttackFinished;

        SpecialAttackSO.Apply(p_owner);
        p_ability.RemoveTag(SpecialAttackSO);
    }

    /// <summary>
    /// 공격 이 끝났을 때 이벤트
    /// </summary>
    private void OnAttackFinished()
    {
        if (p_owner != null)
        {
            SpecialAttackSO.Revert(p_owner);
            p_owner.Events.AttackFinished -= OnAttackFinished;
        }
    }
}
