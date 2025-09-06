using UnityEngine;

public class HeatSystem : MonoBehaviour, IHeatable
{
    [SerializeField] private int _maxHeat = 100;
    [SerializeField] private int _currentHeat = 0;
    [SerializeField] protected HeatDataBase p_heatDataBase;

    public int MaxHeat => _maxHeat;
    public int CurrentHeat => _currentHeat;

    public void ChangeHeat(int amount)
    {
        _currentHeat = Mathf.Clamp(_currentHeat + amount, 0, _maxHeat);
    }
    
    private CalculationResult CalculationHeat(string id, ActorType actorType, int tier, int baseDamage)
    {
        HeatData data = p_heatDataBase.GetHeatData(id, actorType, tier);
        CalculationResult finalStats = StatCalculator.CalculateStats(data, baseDamage);
        return finalStats;
    }
    
}
