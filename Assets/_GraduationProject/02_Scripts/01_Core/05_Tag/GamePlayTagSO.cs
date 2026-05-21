using System;
using UnityEngine;

/// <summary>
/// 게임 플레이 태그 스크립터블 오브젝트
/// 플래그 형태로 사용
/// </summary>
[CreateAssetMenu(fileName = "GamePlayTagSO", menuName = "Project/Tag/GamePlayTag")]
public class GamePlayTagSO : ScriptableObject, IEquatable<GamePlayTagSO>
{
    public string ID;

    public string GetId() => string.IsNullOrEmpty(ID) ? name : ID;

    public virtual void Apply(PlayerController player)
    {

    }


    public virtual void Revert(PlayerController player)
    {

    }

    public override bool Equals(object obj)
    {
        if (obj is GamePlayTagSO other)
        {
            return Equals(other);
        }
        return false;
    }

    public bool Equals(GamePlayTagSO other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetId() == other.GetId();
    }

    public override int GetHashCode()
    {
        return GetId().GetHashCode();
    }

    public static bool operator ==(GamePlayTagSO left, GamePlayTagSO right)
    {
        if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
        return left.Equals(right);
    }

    public static bool operator !=(GamePlayTagSO left, GamePlayTagSO right)
    {
        return !(left == right);
    }
}
