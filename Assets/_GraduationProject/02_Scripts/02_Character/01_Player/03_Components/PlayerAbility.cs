using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbility : MonoBehaviour, IDisposable, IEventListener<PlayerAbilitySO>
{
    [Header("References")]
    private PlayerEvents _events;
    [SerializeField] private HashSet<PlayerAbilitySO> _abilitySet = new HashSet<PlayerAbilitySO>();    // 태그 해시셋
    [SerializeField] private List<PlayerAbilityTagSO> _abilityTags = new List<PlayerAbilityTagSO>();

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    /// <param name="player">플레이어</param>
    public void Initialize(PlayerController player)
    {
        _events = player.Events;


        _events.BeforeDamaged += OnBeforeDamaged;
    }

    /// <summary>
    /// 컴포넌트 해제
    /// </summary>
    public void Dispose()
    {
        _events.BeforeDamaged -= OnBeforeDamaged;

    }

    public void OnEventTrigger(PlayerAbilitySO ability)
    {
        AddAbility(ability);
    }

    //==========================================================================================================================
    // Ability Management ======================================================================================================
    //==========================================================================================================================

    #region Ability Management
    /// <summary>
    /// 기술 추가 함수
    /// </summary>
    /// <param name="ability">추가할 기술</param>
    public void AddAbility(PlayerAbilitySO ability)
    {
        _abilitySet.Add(ability);
        ability.RegisterAbility(this);    // 기술 등록
    }

    /// <summary>
    /// 기술 제거 함수
    /// </summary>
    /// <param name="ability">삭제할 기술</param>
    public void RemoveAbility(PlayerAbilitySO ability)
    {
        _abilitySet.Remove(ability);
        ability.UnregisterAbility(this);  // 기술 해제
    }

    /// <summary>
    /// 기술을 가지고 있는지 확인하는 함수
    /// </summary>
    /// <param name="ability">확인할 기술</param>
    /// <returns>가지고 있는지 여부</returns>
    public bool HasAbility(PlayerAbilitySO ability)
    {
        return _abilitySet.Contains(ability);
    }
    #endregion

    //==========================================================================================================================
    // Tag Management ==========================================================================================================
    //==========================================================================================================================

    #region Tag Management
    /// <summary>
    /// 태그 추가 함수
    /// </summary>
    /// <param name="tag">등록할 태그</param>
    public void AddTag(PlayerAbilityTagSO tag)
    {
        _abilityTags.Add(tag);
    }

    /// <summary>
    /// 태그 제거 함수
    /// </summary>
    /// <param name="tag">등록할 태그</param>
    public void RemoveTag(PlayerAbilityTagSO tag)
    {
        _abilityTags.Remove(tag);
    }

    /// <summary>
    /// 태그를 가지고 있는지 확인하는 함수
    /// </summary>
    /// <param name="id">확인할 태그 아이디</param>
    /// <returns>가지고 있는지 여부</returns>
    public bool HasTag(string id)
    {
        foreach (var tag in _abilityTags)
        {
            // 태그가 비어있으면 계속
            if (tag == null)
            {
                continue;
            }

            if (tag.TagId == id)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 태그를 가지고 있는지 확인하는 함수
    /// </summary>
    /// <param name="validTag">확인할 태그</param>
    /// <returns>가지고 있는지 여부</returns>
    public bool HasTag(PlayerAbilityTagSO validTag)
    {
        foreach (var tag in _abilityTags)
        {
            // 태그가 비어있으면 계속
            if (tag == null)
            {
                continue;
            }

            if (tag == validTag)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 아이디에 맞는 태그 반환
    /// </summary>
    /// <param name="id">태그 아이디</param>
    /// <returns>스킬 태그</returns>
    public PlayerAbilityTagSO GetTag(string id)
    {
        foreach (var tag in _abilityTags)
        {
            // 태그가 비어있으면 계속
            if (tag == null)
            {
                continue;
            }

            if (tag.TagId == id)
            {
                return tag;
            }
        }

        return null;
    }       
    #endregion


    //==========================================================================================================================
    // Event Handler ===========================================================================================================
    //==========================================================================================================================

    /// <summary>
    /// 데미지 받기 전 이벤트 발행
    /// </summary>
    /// <param name="damageContext">받은 데미지 데이터</param>
    private void OnBeforeDamaged(ref PlayerDamageContext damageContext)
    {
        DamageData damageData = damageContext.Data;

        // 무적 태그가 있으면 무적
        if (HasTag("Invicible"))
        {
            damageData.DamageAmount = 0;
            damageData.StiffnessAmount = 0;
            damageData.KnockbackDuration = 0;
            damageData.KnockbackForce = 0;

            damageContext.HasSuperArmor = true;
        }
        else if(HasTag("SuperArmor")) // 슈퍼아머 태그가 있으면 슈퍼아머
        {
            damageContext.HasSuperArmor = true;
        }
    }
}
