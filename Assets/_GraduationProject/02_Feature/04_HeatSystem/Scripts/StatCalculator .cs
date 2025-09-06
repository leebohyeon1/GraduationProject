using UnityEngine;
public struct CalculationResult
{
    public bool IsSuccess;
    public int FinalDamage;
    public float FinalAnimSpeed;
    public float FinalRange;
    public float FinalSpeed;
    public float HeatGauage;
}
public static class StatCalculator
{
    private static TierStatDatabaseSO _tierStatDatabase;

    public static void Initialize(TierStatDatabaseSO tierDb)
    {
        _tierStatDatabase = tierDb;
    }
    
    public static CalculationResult CalculateStats(SourceMap data, int baseDamage)
    {
        CalculationResult result = new CalculationResult();

        if (_tierStatDatabase == null || data == null)
        {
            result.IsSuccess = false;
            return result;
        }

        TierStatData tierStats = _tierStatDatabase.GetTierStat(data.TierID);
        if (tierStats == null)
        {
            result.IsSuccess = false;
            return result;
        }

        // --- 모든 계산을 여기서 한번에 수행 ---
        result.FinalDamage = (int)(baseDamage * tierStats.DamageMultiply);
        result.FinalAnimSpeed = 1.0f * tierStats.AnimSpeedMultiply;
        result.FinalRange = 1.0f * tierStats.RangeMultiply;
        
        result.IsSuccess = true;
        return result;
    }

}