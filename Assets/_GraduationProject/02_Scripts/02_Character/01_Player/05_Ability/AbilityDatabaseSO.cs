using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내 모든 능력을 관리하는 데이터베이스 스크립터블 오브젝트
/// </summary>
[CreateAssetMenu(fileName = "AbilityDatabase", menuName = "Project/Database/AbilityDatabase")]
public class AbilityDatabaseSO : ScriptableObject
{
    [SerializeField] private List<PlayerAbilitySO> _abilities = new List<PlayerAbilitySO>();

    /// <summary>
    /// ID로 능력을 찾아서 반환
    /// </summary>
    public PlayerAbilitySO GetAbility(string id)
    {
        foreach (var ability in _abilities)
        {
            if (ability != null && ability.Id == id)
            {
                return ability;
            }
        }
        return null;
    }

    /// <summary>
    /// (에디터용) 모든 능력을 리스트에 추가하는 기능이 필요하다면 여기에 작성
    /// </summary>
    public List<PlayerAbilitySO> GetAllAbilities()
    {
        return _abilities;
    }
}
