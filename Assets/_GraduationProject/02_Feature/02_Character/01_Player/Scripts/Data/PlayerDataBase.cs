using System;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 플레이어의 데이터를 관리하는 Glue 컴포넌트
/// </summary>
public class PlayerDataBase : MonoBehaviour
{
    #region Private Fields
    [SerializeField] private PlayerBaseDatasSO _baseDatasSO;
    [SerializeField] private TierStatDatabaseSO _tierStatDatabaseSO;
    [SerializeField] private SourceMapDatabaseSO _sourceMapDatabaseSO;

   
    private PlayerData _runtimeData;
    #endregion

    #region Properties
    public PlayerBaseDatasSO BaseData => _baseDatasSO;
    public PlayerData RuntimeData => _runtimeData;
    public TierStatDatabaseSO TierStatData => _tierStatDatabaseSO;
    public SourceMapDatabaseSO SourceMapData => _sourceMapDatabaseSO;
    #endregion

    public void Initialize()
    {
        _runtimeData = new PlayerData();
        _runtimeData.Initialize(_baseDatasSO);
    }

    public void UpdateRunTimeData(int currentHeat)
    {
        TierStatData tierStatData = TierStatData.GetTierStat(currentHeat);

        _runtimeData.UpdateData(BaseData, tierStatData);
    }
}

public class PlayerDataManager : IDisposable
{
    private PlayerDataBase _data;
    private PlayerHeat _heat;
    private PlayerEvents _events;

    public PlayerDataManager(PlayerDataBase data, PlayerHeat heat, PlayerEvents events)
    {
        _data = data;
        _heat = heat;
        _events = events;

        _heat.OnTierChanged += HandleTierChanged;
    }

    public void Dispose()
    {
        _heat.OnTierChanged -= HandleTierChanged;
    }

    /// <summary>
    /// 티어가 바뀌었을 때 효과
    /// </summary>
    /// <param name="previousTier">이전 티어</param>
    /// <param name="currentTier">이전 티어</param>
    public void HandleTierChanged(int previousTier, int currentTier) 
    {
        _data.UpdateRunTimeData(currentTier);
    }
}