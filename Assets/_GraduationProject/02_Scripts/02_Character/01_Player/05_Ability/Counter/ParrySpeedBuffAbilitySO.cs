using System.Collections;
using UnityEngine;

/// <summary>
/// 패링 성공 시 일정 시간 동안 공격 속도를 상승시키는 능력입니다.
/// </summary>
[CreateAssetMenu(fileName = "ParrySpeedBuffAbilitySO", menuName = "Project/Player/Ability/Counter/ParrySpeedBuffAbilitySO")]
public class ParrySpeedBuffAbilitySO : PlayerAbilitySO
{
    [Header("Buff Settings")]
    [SerializeField] private StatModifier _speedModifier; // 증가할 공격 속도 (0.2 = 20% 증가)
    [SerializeField] private float _buffDuration = 10f;        // 버프 지속 시간

    private Coroutine _buffCoroutine;

    public override void RegisterAbility(PlayerAbility ability)
    {
        base.RegisterAbility(ability);
        
        // 패링 성공 이벤트 구독
        if (p_owner != null && p_owner.Events != null)
        {
            p_owner.Events.CounterSucceeded += OnCounterSucceeded;
        }
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        // 이벤트 해제
        if (p_owner != null && p_owner.Events != null)
        {
            p_owner.Events.CounterSucceeded -= OnCounterSucceeded;
        }

        // 버프 중지 및 스탯 원복
        StopBuff();
        
        base.UnregisterAbility(ability);
    }

    private void OnCounterSucceeded(Transform enemy, AttackType type)
    {
        // 버프 시작 또는 갱신
        if (_buffCoroutine != null)
        {
            p_owner.StopCoroutine(_buffCoroutine);
        }

        _buffCoroutine = p_owner.StartCoroutine(BuffRoutine());
    }

    private IEnumerator BuffRoutine()
    {
        // 1. 버프 적용 (이미 적용되어 있다면 Stat 시스템이 중복 추가를 방지하거나, 제거 후 다시 추가)
        // 안전하게 모든 기존 Modifier 제거 후 추가
        p_owner.RuntimeData.AttackSpeed.RemoveAllModifiersFromSource(this);
        p_owner.RuntimeData.AttackSpeed.AddModifier(_speedModifier);

        // 2. 대기
        yield return new WaitForSeconds(_buffDuration);

        // 3. 버프 제거
        StopBuff();
    }

    private void StopBuff()
    {
        if (p_owner != null && p_owner.RuntimeData != null)
        {
            p_owner.RuntimeData.AttackSpeed.RemoveAllModifiersFromSource(this);
        }

        if (_buffCoroutine != null && p_owner != null)
        {
            p_owner.StopCoroutine(_buffCoroutine);
            _buffCoroutine = null;
        }
        
        Debug.Log("[Ability] 공격 속도 버프 종료");
    }
}
