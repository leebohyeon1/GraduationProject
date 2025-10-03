using System;
using UnityEngine;



/// <summary>
/// 플레이어의 데이터를 모아놓은 컴포넌트
/// </summary>
[CreateAssetMenu(fileName = "PlayerDatasBaseSO", menuName = "Player/PlayerDatasBaseSO")]
public class PlayerDataBaseSO : ScriptableObject
{
    #region Private Fields
    [SerializeField] private BasePlayerDatasSO _baseDatasSO;
    [SerializeField] private TierStatDatabaseSO _tierStatDatabaseSO;
    [SerializeField] private SourceMapDatabaseSO _sourceMapDatabaseSO;
    [SerializeField] private OverHeatDataSO _overHeatDataSO;

    #endregion

    #region Properties
    public BasePlayerDatasSO BaseData => _baseDatasSO;
    public TierStatDatabaseSO TierStatData => _tierStatDatabaseSO;
    public SourceMapDatabaseSO SourceMapData => _sourceMapDatabaseSO;
    public OverHeatDataSO OverHeatData => _overHeatDataSO;
    #endregion

}

[CreateAssetMenu(fileName = "BasePlayerDatasSO", menuName = "Player/BasePlayerDatasSO")]
public class BasePlayerDatasSO : ScriptableObject
{
    [Header("Stats")]
    public int MaxHealth = 100;

    [Header("Movement")]
    public LayerMask GroundLayerMask = 1 << 3;
    public float MoveSpeed = 5f;
    public float RotateSpeed = 5f;
    public float Gravity = -9.81f;
    public float GroundCheckDistance = 0.1f;

    [Header("Combat")]
    public PlayerCombatData CombatData;
}
