using System.Collections;
using UnityEngine;

/// <summary>
/// 인파이팅 4티어
/// 제한시간 내에 
/// 상쇄 3회 연속 성공시 각성
/// 데미지가 들어오면 실패
/// </summary>
[CreateAssetMenu(fileName = "BerserkSO", menuName = "Project/Player/Ability/TheDeullist/Tier4/BerserkSO")]
public class BerserkSO : PlayerAbilitySO
{
    public float TimeLimit;         // 제한 시간
    public int CounterThreshold;    // 연속 상쇄 임계값

    public float BerserkerTime;     // 폭주 시간

    private int _counterCount;  // 연속 상쇄 횟수
    private Coroutine _counterCoroutine;

    private bool _isBerserker;
    private Coroutine _berserkerCoroutine;

    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();

        p_owner.Events.CounterSucceeded += OnCounterSucceded;
        p_owner.Health.TakeDamged += OnTakeDamaged;
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_owner.Events.CounterSucceeded -= OnCounterSucceded;
        p_owner.Health.TakeDamged -= OnTakeDamaged;

        p_ability = null;
        p_owner = null;

        _counterCoroutine = null;
        _berserkerCoroutine = null;
    }
    
    private void OnCounterSucceded(Transform transform)
    {
        // 폭주 상태면 리턴
        if(_isBerserker)
        {
            return;
        }

        if (_counterCoroutine == null)
        {
            _counterCoroutine = p_owner.StartCoroutine(UpdateTime());
        }

        _counterCount++;

        // 연속 상쇄가 연속 상쇄 임계값 이상 시
        if (_counterCount >= CounterThreshold)
        {
            Berserker();

            if (_counterCoroutine != null)
            {
                p_owner.StopCoroutine(_counterCoroutine);
            }
        }
    }

    private void OnTakeDamaged(int amount)
    {
        CountReset();
    }

    private IEnumerator UpdateTime()
    {
        yield return new WaitForSeconds(TimeLimit);

        CountReset();
    }

    /// <summary>
    /// 도전 실패
    /// </summary>
    private void CountReset()
    {
        _counterCount = 0;
        _counterCoroutine = null;
    }

    /// <summary>
    /// 폭주 함수
    /// </summary>
    private void Berserker()
    {
        if(_berserkerCoroutine == null)
        {
            _berserkerCoroutine = p_owner.StartCoroutine(BerserkerCorountine());

            // 횟수 초기화
            CountReset();
        }
    }

    private IEnumerator BerserkerCorountine()
    {
        _isBerserker = true;
        AddAllSkillTags();

        yield return new WaitForSeconds(BerserkerTime);

        _isBerserker = false;
        RemoveAllSkillTags();

        _berserkerCoroutine = null;
    }

}
