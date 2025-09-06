using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HeatDataManager", menuName = "GameData/HeatDataManager")]
public class HeatDataManager : ScriptableObject
{
    public List<HeatDataBase> HeatDataBases;
    public List<TierStatDatabase> TierStatDatabases;
}
