using UnityEngine;

public class HeatSystem : MonoBehaviour, IHeatable
{
    [SerializeField] private int _maxHeat = 100;
    [SerializeField] private int _currentHeat = 0;
    [SerializeField] private HeatDataBase _heatDataBase;

    public int maxHeat => _maxHeat;
    public int currentHeat => _currentHeat;

    public void ChangeHeat(int amount)
    {
        _currentHeat = Mathf.Clamp(_currentHeat + amount, 0, _maxHeat);
    }
    private CalculationResult heatdata(string interactorId, ActorType actorType, int tier,int baseDamage)
    {
        HeatData data = _heatDataBase.GetHeatData(interactorId, actorType, tier);
        CalculationResult finalStats = StatCalculator.CalculateStats(data, baseDamage);
        return finalStats;
    }
    
}
