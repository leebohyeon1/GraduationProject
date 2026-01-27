using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 업그레이드 스크립터블 오브젝트
/// 플레이어에게 이벤트를 받아 태그를 주고 받음
/// </summary>
[CreateAssetMenu(fileName = "PlayerAbilitySO", menuName = "Project/Player/Ability/PlayerAbilitySO")]
public class PlayerAbilitySO : ScriptableObject
{
    protected PlayerController p_owner;
    protected PlayerAbility p_ability;  // 능력 주체
    public List<PlayerAbilityTagSO> Tags;    // 이 능력이 부여하는 태그들

    /// <summary>
    /// 기능 등록
    /// </summary>
    /// <param name="ability">플레이어</param>
    public virtual void RegisterAbility(PlayerAbility ability)
    {
        p_ability = ability;
        p_owner = p_ability.GetComponent<PlayerController>();   

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
    }

    /// <summary>
    /// 모든 스킬 태그 추가
    /// </summary>
    protected virtual void AddAllSkillTags()
    {
        foreach (var tag in Tags)
        {
            tag.Apply(p_owner);     // 태그 적용
            p_ability.AddTag(tag);  // 어빌리티에 추가
        }
    }

    /// <summary>
    /// 모든 스킬 태그 제거
    /// </summary>
    protected virtual void RemoveAllSkillTags()
    {
        foreach (var tag in Tags)
        {
            tag.Revert(p_owner);        // 태그 해제
            p_ability.RemoveTag(tag);   // 어빌리티에 제거
        }
    }
}