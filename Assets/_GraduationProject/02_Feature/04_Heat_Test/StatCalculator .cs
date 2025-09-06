using UnityEngine;
public struct CalculationResult
{
    public bool isSuccess;
    public int finalDamage;
    public float finalAnimSpeed;
    public float finalRange;
    public float finalSpeed;
    public float HeatGauage;
}
public static class StatCalculator
{
    private static TierStatDatabase _tierStatDatabase;

    public static void Initialize(TierStatDatabase tierDb)
    {
        _tierStatDatabase = tierDb;
    }
    
    public static CalculationResult CalculateStats(HeatData rule, int baseDamage)
    {
        CalculationResult result = new CalculationResult();

        if (_tierStatDatabase == null || rule == null)
        {
            result.isSuccess = false;
            return result;
        }

        TierStatData tierStats = _tierStatDatabase.GetTierStat(rule.TierID);
        if (tierStats == null)
        {
            result.isSuccess = false;
            return result;
        }

        // --- 모든 계산을 여기서 한번에 수행 ---
        result.finalDamage = (int)(baseDamage * tierStats.DamageMultiply);
        result.finalAnimSpeed = 1.0f * tierStats.AnimSpeedMultiply;
        result.finalRange = 1.0f * tierStats.RangeMultiply;
        
        result.isSuccess = true;
        return result;
    }

}