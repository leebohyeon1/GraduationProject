using System;
using Unity.Collections;
using UnityEngine;

public class HeatSystem : MonoBehaviour, IHeatable
{
    [SerializeField] private int _maxHeat = 100;
    [SerializeField] private int _currentHeat = 0;
    [SerializeField] protected SourceMapDatabaseSO p_heatDataBase;
    [SerializeField] protected TierStatDatabaseSO p_tierStatDatabase;

    public event Action<int, int> OnHeatChanged;
    public event Action<int, int> OnTierChanged;

    public int MaxHeat => _maxHeat;
    public int CurrentHeat => _currentHeat;
    public int CurrentTier => GetTier();

    [field: SerializeField]
    public ActorType ActorType { get; private set; }

    public void ChangeHeat(int amount)
    {
        if (amount == 0) return;

        int oldTier = GetTier();
        int oldHeat = _currentHeat;

        _currentHeat = Mathf.Clamp(_currentHeat + amount, 0, _maxHeat);

        if (oldHeat != _currentHeat)
        {
            OnHeatChanged?.Invoke(_currentHeat, _maxHeat);
        }

        int newTier = GetTier();
        if (oldTier != newTier)
        {
            OnTierChanged?.Invoke(oldTier, newTier);
        }
    }

    protected CalculationResult CalculationHeat(string id, ActorType actorType, int tier, int baseDamage)
    {
        SourceMap data = p_heatDataBase.GetSourceMap(id, actorType, tier);
        CalculationResult finalStats = StatCalculator.CalculateStats(data, baseDamage);
        return finalStats;
    }

    public int GetTier()
    {
        return p_tierStatDatabase.GetCurrentTier(CurrentHeat);
    }
}
