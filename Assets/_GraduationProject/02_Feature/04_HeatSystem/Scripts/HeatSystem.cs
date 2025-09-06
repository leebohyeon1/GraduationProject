using System;
using Unity.Collections;
using UnityEngine;

public class HeatSystem : MonoBehaviour, IHeatable
{
    [SerializeField] protected int p_maxHeat = 100;
    [SerializeField] private int _currentHeat = 0;
    [SerializeField] protected HeatDataBase p_heatDataBase;
    [SerializeField] protected TierStatDatabase p_tierStatDatabase;

    public event Action<int, int> OnHeatChanged;
    public event Action<int, int> OnTierChanged;

    public int MaxHeat => p_maxHeat;
    public int CurrentHeat => _currentHeat;
    public int CurrentTier => GetTier();

    public void ChangeHeat(int amount)
    {
        if (amount == 0) return;

        int oldTier = GetTier();
        int oldHeat = _currentHeat;

        _currentHeat = Mathf.Clamp(_currentHeat + amount, 0, p_maxHeat);

        if (oldHeat != _currentHeat)
        {
            OnHeatChanged?.Invoke(_currentHeat, p_maxHeat);
        }

        int newTier = GetTier();
        if (oldTier != newTier)
        {
            OnTierChanged?.Invoke(oldTier, newTier);
        }
    }

    private CalculationResult CalculationHeat(string id, ActorType actorType, int tier, int baseDamage)
    {
        HeatData data = p_heatDataBase.GetHeatData(id, actorType, tier);
        CalculationResult finalStats = StatCalculator.CalculateStats(data, baseDamage);
        return finalStats;
    }

    public int GetTier()
    {
        return p_tierStatDatabase.GetCurrentTier(CurrentHeat);
    }
}
