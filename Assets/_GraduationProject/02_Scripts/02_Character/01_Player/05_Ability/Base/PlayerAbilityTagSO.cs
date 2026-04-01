using NUnit.Framework;
using UnityEngine;

/// <summary>
/// 플레이어의 기술 태그 스크립터블 오브젝트
/// </summary>
[CreateAssetMenu(fileName = "PlayerAbilityTagSO", menuName = "Project/Player/Ability/Tag/PlayerAbilityTagSO", order = 0)]
public class PlayerAbilityTagSO : ScriptableObject
{
    public string Id;

    public virtual void Apply(PlayerController player)
    {

    }


    public virtual void Revert(PlayerController player)
    {

    }
}
