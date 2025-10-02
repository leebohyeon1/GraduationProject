using System;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 플레이어의 데이터를 관리하는 Glue 컴포넌트
/// </summary>
public class PlayerDataBase : MonoBehaviour
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