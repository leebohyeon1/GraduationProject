using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인파이팅 1티어
/// 피격 후 일정 시간 동안 공격 데미지의 일정 비율 만큼 회복
/// 받은 데미지 이상으로 회복되지 않으며, 각 데미지는 개별적으로 관리됨
/// </summary>
[CreateAssetMenu(fileName = "RegainSO", menuName = "Project/Player/Ability/TheDeullist/Tier1/RegainSO")]
public class RegainSO : PlayerAbilitySO
{
    public float RegainTime;    // 각 데미지별 회복 유효 시간

    // 개별 데미지 정보를 담는 클래스
    private class RegainInstance
    {
        public int RemainingAmount; // 남은 회복 가능 잔액
        public float ExpiryTime;    // 만료 시간

        public RegainInstance(int amount, float duration)
        {
            RemainingAmount = amount;
            ExpiryTime = Time.time + duration;
        }

        public bool IsValid => RemainingAmount > 0 && Time.time < ExpiryTime;
    }

    private List<RegainInstance> _activeRegains = new List<RegainInstance>();
    private Coroutine _cleanupCoroutine;

    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();

        p_owner.Health.TakeDamged += OnTakeDamaged;
        
        // 필터 대리자에 등록하여 실제 발생할 흡혈량을 제어합니다.
        p_owner.Events.FilterAttackRegain += FilterRegainAmount;
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        if (p_owner != null)
        {
            p_owner.Health.TakeDamged -= OnTakeDamaged;
            p_owner.Events.FilterAttackRegain -= FilterRegainAmount;
        }
        
        if (_cleanupCoroutine != null)
        {
            p_owner.StopCoroutine(_cleanupCoroutine);
            _cleanupCoroutine = null;
        }

        _activeRegains.Clear();
        p_ability = null;
        p_owner = null;   
    }

    private void OnTakeDamaged(int damage)
    {
        // 새로운 데미지 인스턴스 추가
        _activeRegains.Add(new RegainInstance(damage, RegainTime));
        
        // 회복 가능 상태이므로 태그 활성화
        AddAllSkillTags();

        // 관리 코루틴 시작 (만료된 인스턴스 제거용)
        if (_cleanupCoroutine == null)
        {
            _cleanupCoroutine = p_owner.StartCoroutine(CleanupRoutine());
        }
    }

    // [핵심] 실제 발생할 흡혈량을 필터링합니다.
    private int FilterRegainAmount(int requestedAmount)
    {
        int finalHealAmount = 0;
        int remainingToRecover = requestedAmount;

        // 유효한 인스턴스들에서 순차적으로 잔액을 소진하며 회복량을 결정
        for (int i = 0; i < _activeRegains.Count; i++)
        {
            if (remainingToRecover <= 0) break;

            if (_activeRegains[i].IsValid)
            {
                int takeFromThis = Mathf.Min(_activeRegains[i].RemainingAmount, remainingToRecover);
                _activeRegains[i].RemainingAmount -= takeFromThis;
                remainingToRecover -= takeFromThis;
                finalHealAmount += takeFromThis;
            }
        }

        // 모든 잔액이 소진되었는지 체크하여 태그 관리
        CheckAndRemoveTags();

        return finalHealAmount;
    }
        
    private IEnumerator CleanupRoutine()
    {
        while (_activeRegains.Count > 0)
        {
            // 만료되거나 소진된 인스턴스 제거
            _activeRegains.RemoveAll(r => !r.IsValid);
            
            CheckAndRemoveTags();

            if (_activeRegains.Count == 0) break;

            yield return new WaitForSeconds(0.1f); // 주기적으로 체크
        }

        _cleanupCoroutine = null;
    }

    private void CheckAndRemoveTags()
    {
        bool hasValid = false;
        foreach (var r in _activeRegains)
        {
            if (r.IsValid)
            {
                hasValid = true;
                break;
            }
        }

        if (!hasValid)
        {
            RemoveAllSkillTags();
        }
    }
}
