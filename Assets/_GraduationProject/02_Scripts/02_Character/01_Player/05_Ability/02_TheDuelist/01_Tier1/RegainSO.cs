using DG.Tweening;
using System.Collections;
using UnityEngine;

/// <summary>
/// 인파이팅 1티어
/// 피격 후 일정 시간 동안 공격 데미지의 일정 비율 만큼 회복
/// </summary>
[CreateAssetMenu(fileName = "RegainSO", menuName = "Project/Player/Ability/TheDeullist/Tier1/RegainSO")]
public class RegainSO : PlayerAbilitySO
{
    public float RegainTime;    // 회복 시간

    private Coroutine regainCoroutine;

    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();

        p_owner.Health.TakeDamged += OnTakeDamaged;
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_ability = null;
        p_owner = null;
        
        p_owner.Health.TakeDamged -= OnTakeDamaged;
    }

    private void OnTakeDamaged(int damage)
    {
        if (regainCoroutine != null)
        {
            RemoveAllSkillTags();
            p_owner.StopCoroutine(regainCoroutine);
            regainCoroutine = null;
        }

        regainCoroutine = p_owner.StartCoroutine(RegainCoroutine());
    }
        
    private IEnumerator RegainCoroutine()
    {
        AddAllSkillTags();

        yield return new WaitForSeconds(RegainTime);

        RemoveAllSkillTags();
        regainCoroutine = null;
    }
}
