using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CounterDurationAbility", menuName = "Project/Player/Ability/Ability/CounterDurationAbility")]
public class CounterDurationAbility : PlayerAbilitySO
{
    [SerializeField] private float counterBuffDuration = 5f; // 카운터 버프 지속 시간

    private Coroutine counterBuffCoroutine;

    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();
        p_tagInstances = new List<PlayerAbilityTagSO>();

        p_owner.Events.CounterSucceeded += OnCounterSucceeded;
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_owner.Events.CounterSucceeded -= OnCounterSucceeded;

        base.UnregisterAbility(ability);
    }

    private void OnCounterSucceeded(Transform transform, AttackType type)
    {
        if (type == AttackType.Normal_Counter || type == AttackType.Strong_Counter)
        {
            // 카운터 성공 시 적용할 효과 추가
            // 예: 체력 회복, 버프 적용 등

            if (counterBuffCoroutine == null)
            {
                AddAllSkillTags();
                counterBuffCoroutine = p_owner.StartCoroutine(CounterBuffCoroutine());
            }
        }
    }

    private IEnumerator CounterBuffCoroutine()
    {
        // 카운터 버프 지속 시간 동안 대기
        yield return new WaitForSeconds(counterBuffDuration);
        // 버프 효과 종료 시 필요한 로직 추가 (예: 상태 초기화)
        RemoveAllSkillTags();
        counterBuffCoroutine = null;
    }
}
