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
        ability.ApplyAbility(gameObject);
    }

}
