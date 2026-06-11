using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chain", menuName = "Project/Player/Ability/Ability/Chain")]
public class Chain : PlayerAbilitySO
{
    [SerializeField] private float _chainDuration = 8f;
    [SerializeField] private int _maxChainCount = 10;
    private int _chainCount = 0;

    private Coroutine _chaingCoroutine;


    public override void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();
        p_tagInstances = new List<PlayerAbilityTagSO>();

        p_owner.Combat.AttackEvent += OnAttackEvent;
    }

    public override void UnregisterAbility(PlayerAbility ability)
    {
        p_ability = null;
        p_owner = null;
        
        p_owner.Combat.AttackEvent -= OnAttackEvent;

        p_tagInstances = null;
    }

    private void OnAttackEvent(IDamageable damageable, DamageData data)
    {
        if(_chainCount >= _maxChainCount)
        {
            return; // 최대 체인 수에 도달한 경우 더 이상 적용하지 않음
        }

        if(data.AttackType <= AttackType.Normal_3)
        {
            if (_chaingCoroutine == null)
            {
                _chaingCoroutine = p_owner.StartCoroutine(ChainCoroutine());
            }

            AddAllSkillTags();
            _chainCount++;
        }
    }

    private IEnumerator ChainCoroutine()
    {
        // 체인 지속 시간 동안 대기
      
        yield return new WaitForSeconds(_chainDuration);

        // 체인 효과 종료 시 필요한 로직 추가 (예: 상태 초기화)
        _chainCount = 0;
        RemoveAllSkillTags();
        _chaingCoroutine = null;
    }
}
