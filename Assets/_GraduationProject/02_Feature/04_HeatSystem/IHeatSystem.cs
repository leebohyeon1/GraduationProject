using System;
using UnityEngine;

public interface IHeatable
{
    public int MaxHeat { get; }
    public int CurrentHeat { get; }
    public int CurrentTier { get; }
    public event Action<int, int> OnHeatChanged;
    public event Action<int, int> OnTierChanged;
    public void ChangeHeat(int amount);
    public int GetTier();
}

public interface IOverHeatable
{
    public int TriggerThrehold { get; }
    public float DelaySecond { get; }
    public float TickSecond { get; }
    public int DamagePerTick { get; }
    public int MaxHpRatioDamage { get; }
    public float GroggySecond { get; }
    public bool IsHeatLock { get; }

    public void OverHeat();
}
