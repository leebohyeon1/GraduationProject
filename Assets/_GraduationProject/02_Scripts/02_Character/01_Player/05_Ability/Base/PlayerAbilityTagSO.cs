using System;
using UnityEngine;

/// <summary>
/// 플레이어의 기술 태그 스크립터블 오브젝트
/// </summary>
[CreateAssetMenu(fileName = "PlayerAbilityTagSO", menuName = "Project/Player/Ability/Tag/PlayerAbilityTagSO", order = 0)]
public class PlayerAbilityTagSO : ScriptableObject, IEquatable<PlayerAbilityTagSO>
{
    public string Id;

    public string GetId() => string.IsNullOrEmpty(Id) ? name : Id;

    public virtual void Apply(PlayerController player)
    {

    }


    public virtual void Revert(PlayerController player)
    {

    }

    public override bool Equals(object obj)
    {
        if (obj is PlayerAbilityTagSO other)
        {
            return Equals(other);
        }
        return false;
    }

    public bool Equals(PlayerAbilityTagSO other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetId() == other.GetId();
    }

    public override int GetHashCode()
    {
        return GetId().GetHashCode();
    }

    public static bool operator ==(PlayerAbilityTagSO left, PlayerAbilityTagSO right)
    {
        if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
        return left.Equals(right);
    }

    public static bool operator !=(PlayerAbilityTagSO left, PlayerAbilityTagSO right)
    {
        return !(left == right);
    }
}
