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
    /// 주어진 열 값에 대해 달성된 가장 높은 등급을 가져옵니다.
    /// </summary>
    /// <param name="currentHeat">현재 열 값입니다.</param>
    /// <returns>달성한 가장 높은 등급의 ID입니다. 충족되는 등급 임계값이 없으면 0을 반환합니다.</returns>
    public int GetCurrentTier(int currentHeat)
    {
        int highestAchievedTier = 0;

        // 리스트의 모든 등급을 순회합니다.
        foreach (var tierData in TierStats)
        {
            // 현재 열이 등급의 임계값을 넘었고,
            // 이 등급이 이전에 찾은 가장 높은 등급보다 높다면
            if (currentHeat >= tierData.HeatThrehold && tierData.TierID > highestAchievedTier)
            {
                // 가장 높은 등급을 현재 등급으로 업데이트합니다.
                highestAchievedTier = tierData.TierID;
            }
        }

        // 루프가 끝난 후, 찾은 가장 높은 등급을 반환합니다.
        return highestAchievedTier;
    }
}
