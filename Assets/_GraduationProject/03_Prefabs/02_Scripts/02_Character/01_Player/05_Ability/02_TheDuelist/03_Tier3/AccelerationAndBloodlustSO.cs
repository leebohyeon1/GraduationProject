using System.Collections;
using UnityEngine;

/// <summary>
/// 인파이팅 3티어
/// 상쇄 성공 시 공속, 공격력 업
/// </summary>
[CreateAssetMenu(fileName = "AccelerationAndBloodlustSO", menuName = "Project/Player/Ability/TheDeullist/Tier3/AccelerationAndBloodlustSO")]
public class AccelerationAndBloodlustSO : PlayerAbilitySO
{
    public float buffTime = 5;
    public int maxStack = 3;

    private int stackCount = 0;
    private Coroutine _buffCoroutine;

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
        if (_buffCoroutine != null)
        {
            p_owner.StopCoroutine(_buffCoroutine);
            _buffCoroutine = null;
        }

        _buffCoroutine = p_owner.StartCoroutine(UpdateBuff());
    }

    private IEnumerator UpdateBuff()
    {
        // 최대 스택보다 적으면 태그 적용
        // 최대 스택이면 시간만 연장
        if (stackCount < maxStack)
        {
            stackCount++;
            AddAllSkillTags();
        }

        yield return new WaitForSeconds(buffTime);

        // 모든 태그 제거
        for (int i = 0; i < stackCount; i++)
        {
            RemoveAllSkillTags();
        }

        // 스택 밑 코루틴 초기화
        stackCount = 0;
        _buffCoroutine = null;
    }
}
