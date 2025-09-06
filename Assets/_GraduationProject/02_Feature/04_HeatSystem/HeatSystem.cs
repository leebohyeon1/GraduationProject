using Unity.Collections;
using UnityEngine;

public class HeatSystem : MonoBehaviour, IHeatable
{
    [SerializeField] protected int p_maxHeat = 100;
    [SerializeField] private int _currentHeat = 0;
    [SerializeField] protected HeatDataBase p_heatDataBase;

    public int MaxHeat => p_maxHeat;
    public int CurrentHeat => _currentHeat;

    public void ChangeHeat(int amount)
    {
        _currentHeat = Mathf.Clamp(CurrentHeat + amount, 0, MaxHeat);
    }
    
    private CalculationResult CalculationHeat(string id, ActorType actorType, int tier, int baseDamage)
    {
        HeatData data = p_heatDataBase.GetHeatData(id, actorType, tier);
        CalculationResult finalStats = StatCalculator.CalculateStats(data, baseDamage);
        return finalStats;
    }
    
}
