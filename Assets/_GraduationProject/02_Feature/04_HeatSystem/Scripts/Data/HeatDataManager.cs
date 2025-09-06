using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HeatDataManager", menuName = "GameData/HeatDataManager")]
public class HeatDataManager : ScriptableObject
{
    public List<SourceMapDatabaseSO> HeatDataBases;
    public List<TierStatDatabaseSO> TierStatDatabases;
}
