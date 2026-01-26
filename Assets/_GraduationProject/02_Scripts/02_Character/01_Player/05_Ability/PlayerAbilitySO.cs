using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 업그레이드 스크립터블 오브젝트
/// 플레이어에게 이벤트를 받아 태그를 주고 받음
/// </summary>
[CreateAssetMenu(fileName = "PlayerAbilitySO", menuName = "Project/Player/Ability/PlayerAbilitySO")]
public class PlayerAbilitySO : ScriptableObject
{
    protected PlayerAbility p_owner;  // 능력 주체
    public List<PlayerAbilityTagSO> Tags;    // 이 능력이 부여하는 태그들

    /// <summary>
    /// 기능 등록
    /// </summary>
    /// <param name="ability">플레이어</param>
    public virtual void RegisterAbility(PlayerAbility ability)
    {
        p_owner = ability;
        AddAllSkillTags();
    }

    /// <summary>
    /// 기능 해제
    /// </summary>
    /// <param name="ability">플레이어</param>
    public virtual void UnregisterAbility(PlayerAbility ability)
    {
        p_owner = null;
        RemoveAllSkillTags();
    }

    /// <summary>
    /// 모든 스킬 태그 추가
    /// </summary>
    protected virtual void AddAllSkillTags()
    {
        foreach (var tag in Tags)
        {
            p_owner.AddTag(tag);
        }
    }

    /// <summary>
    /// 모든 스킬 태그 제거
    /// </summary>
    protected virtual void RemoveAllSkillTags()
    {
        foreach (var tag in Tags)
        {
            p_owner.RemoveTag(tag);
        }
    }
}