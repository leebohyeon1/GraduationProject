using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TierStatData
{
    public int TierID;
    public int HeatThrehold;
    public float SpeedMultiply;
    public float DamageMultiply;
    public float AnimSpeedMultiply;
    public float RangeMultiply;

}

[CreateAssetMenu(fileName = "TierStatDatabase", menuName = "GameData/TierStatDatabase")]
public class TierStatDatabaseSO : ScriptableObject
{
    public List<TierStatData> TierStats;
    public TierStatData GetTierStat(int tierID = 0)
    {
        return TierStats.Find(data => data.TierID == tierID);
    }

    /// <summary>
    /// 주어진 체력/열 값에 대해 달성된 가장 높은 등급을 가져옵니다.
    /// 현재 체력보다 작거나 같은 가장 높은 임계값을 가진 등급을 찾습니다.
    /// </summary>
    /// <param name="currentHealth">현재 체력 또는 열 값입니다.</param>
    /// <returns>달성한 가장 높은 등급의 ID입니다. 충족되는 등급 임계값이 없으면 0을 반환합니다.</returns>
    public int GetCurrentTier(int currentHealth)
    {
        TierStatData bestTier = null;
        foreach (var tierData in TierStats)
        {
            if (currentHealth >= tierData.HeatThrehold)
            {
                if (bestTier == null || tierData.HeatThrehold > bestTier.HeatThrehold)
                {
                    bestTier = tierData;
                }
            }
        }

        if (bestTier != null)
        {
            return bestTier.TierID;
        }

        return 0;
    }
}
