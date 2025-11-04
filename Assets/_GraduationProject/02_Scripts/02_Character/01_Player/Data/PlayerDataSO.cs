using System;
using UnityEngine;

/// <summary>
/// 플레이어의 모든 데이터를 통합 관리하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "PlayerDatasBaseSO", menuName = "Player/PlayerDatasBaseSO")]
public class PlayerDataBaseSO : ScriptableObject
{
    #region Private Fields
    [SerializeField] private BasePlayerDatasSO _baseDatasSO; // 플레이어 기본 데이터


    #endregion

    #region Properties
    // 기본 데이터 접근자
    public BasePlayerDatasSO BaseData => _baseDatasSO;

    #endregion
}
