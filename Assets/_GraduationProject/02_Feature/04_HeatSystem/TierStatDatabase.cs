using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TierStatData
{
    public int TierID;
    public float SpeedMultiply;
    public float DamageMultiply;
    public float AnimSpeedMultiply;
    public float RangeMultiply;
    public int HeatGauage;
}
[CreateAssetMenu(fileName = "TierStatDatabase", menuName = "GameData/TierStatDatabase")]
public class TierStatDatabase : ScriptableObject
{
    public List<TierStatData> TierStats;
    public TierStatData GetTierStat(int tierID = 0)
    {
        return TierStats.Find(data => data.TierID == tierID);
    }
}
