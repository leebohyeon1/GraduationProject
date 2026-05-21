using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 업그레이드 스크립터블 오브젝트
/// 플레이어에게 이벤트를 받아 태그를 주고 받음
/// </summary>
[CreateAssetMenu(fileName = "PlayerAbilitySO", menuName = "Project/Player/Ability/PlayerAbilitySO")]
public class PlayerAbilitySO : ScriptableObject, IEquatable<PlayerAbilitySO>
{
    public string Id;            // 능력 ID

    public string GetId() => string.IsNullOrEmpty(Id) ? name : Id;
    
    protected PlayerController p_owner;
    protected PlayerAbility p_ability;  // 능력 주체
    public List<PlayerAbilityTagSO> Tags;    // 이 능력이 부여하는 태그들
    protected List<PlayerAbilityTagSO> p_tagInstances;

    public override bool Equals(object obj)
    {
        if (obj is PlayerAbilitySO other)
        {
            return Equals(other);
        }
        return false;
    }

    public bool Equals(PlayerAbilitySO other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetId() == other.GetId();
    }

    public override int GetHashCode()
    {
        return GetId().GetHashCode();
    }

    public static bool operator ==(PlayerAbilitySO left, PlayerAbilitySO right)
    {
        if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
        return left.Equals(right);
    }

    public static bool operator !=(PlayerAbilitySO left, PlayerAbilitySO right)
    {
        return !(left == right);
    }

    /// <summary>
    /// 기능 등록
    /// </summary>
    /// <param name="ability">플레이어</param>
    public virtual void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();
        p_tagInstances = new List<PlayerAbilityTagSO>();

        AddAllSkillTags();
    }

    /// <summary>
    /// 기능 해제
    /// </summary>
    /// <param name="ability">플레이어</param>
    public virtual void UnregisterAbility(PlayerAbility ability)
    {
        p_ability = null;
        p_owner = null;

        RemoveAllSkillTags();

        p_tagInstances = null;
    }

    /// <summary>
    /// 모든 스킬 태그 추가
    /// </summary>
    protected virtual void AddAllSkillTags()
    {
        foreach (var tag in Tags)
        {
             PlayerAbilityTagSO instance = Instantiate(tag);
            instance.Apply(p_owner);
            p_ability.AddTag(instance);
            p_tagInstances.Add(instance); // 나중에 Revert를 위해 저장
        }
    }

    /// <summary>
    /// 모든 스킬 태그 제거
    /// </summary>
    protected virtual void RemoveAllSkillTags()
    {
        foreach (var instance in p_tagInstances)
        {
            instance.Revert(p_owner);        // 태그 해제
            p_ability.RemoveTag(instance);   // 어빌리티에 제거
            Destroy(instance);              // 인스턴스 제거   
        }

        p_tagInstances.Clear();
    }
}