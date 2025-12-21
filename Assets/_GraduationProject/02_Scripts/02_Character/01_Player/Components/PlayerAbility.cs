using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbility : MonoBehaviour, IDisposable, IEventListener<AbilitySO>
{
    [SerializeField] private AbilitySelectedSO _abilitySelectedSO;
    private List<AbilitySO> _acquiredAbilities = new List<AbilitySO>();

    private PlayerStats _stats;

    public void Initialize(PlayerStats stat)
    {
        _stats = stat;
        _abilitySelectedSO.Subscribe(this);
    }

    public void Dispose()
    {
       _abilitySelectedSO.Unsubscribe(this);
    }

    public void OnEventTrigger(AbilitySO eventName)
    {
        AddAbility(eventName);
    }

    /// <summary>
    /// 새로운 능력을 추가하고 적용합니다.
    /// </summary>
    /// <param name="ability">선택된 능력 데이터</param>
    public void AddAbility(AbilitySO ability)
    {
        _acquiredAbilities.Add(ability);
        ApplyAbilityEffect(ability);
    }

    private void ApplyAbilityEffect(AbilitySO ability)
    {
        if (_stats == null)
        {
            return;
        }

        switch (ability.Type)
        {
            case AbilityType.StatBoost:
                Debug.Log($"스탯 강화 적용: {ability.AbilityName}");
                _stats.StatUpgrade(ability.PlusStat);

                break;
            case AbilityType.NewSkill:
                Debug.Log($"새로운 스킬 획득: {ability.AbilityName}");
                // 예시: if (ability.SkillPrefab != null) Instantiate(ability.SkillPrefab, transform);
                break;
            case AbilityType.WeaponUpgrade:
                Debug.Log($"무기 업그레이드: {ability.AbilityName}");
                // 예시: GetComponent<WeaponController>().UpgradeWeapon(ability);
                break;
        }
    }

}
