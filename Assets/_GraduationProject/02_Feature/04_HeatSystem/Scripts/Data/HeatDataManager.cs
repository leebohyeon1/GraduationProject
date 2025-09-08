using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HeatDataManager", menuName = "GameData/HeatDataManager")]
public class HeatDataManager : ScriptableObject
{
    public List<SourceMapDatabaseSO> SourceMapDataBases;
    public List<TierStatDatabaseSO> TierStatDatabases;
}
