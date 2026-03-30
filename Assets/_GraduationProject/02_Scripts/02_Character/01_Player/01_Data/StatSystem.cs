using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatModifierType
{
    Flat = 100,      // 더하기 (예: 공격력 +10)
    PercentAdd = 200, // 퍼센트 합산 (예: 공격력 +10%, +20% -> +30%)
    PercentMult = 300 // 최종 곱산 (예: 최종 데미지 2배)
}

[Serializable]
public class StatModifier
{
    public float Value;
    public StatModifierType Type;
    public object Source;

    public StatModifier(float value, StatModifierType type, object source = null)
    {
        Value = value;
        Type = type;
        Source = source;
    }
}

[Serializable]
public class Stat
{
    private Func<float> _baseValueProvider; 
    public float BaseOffset;               

    public float BaseValue => (_baseValueProvider?.Invoke() ?? 0) + BaseOffset;

    private readonly List<StatModifier> _modifiers = new List<StatModifier>();
    public IReadOnlyList<StatModifier> Modifiers => _modifiers.AsReadOnly();

    private bool _isDirty = true;
    private float _lastValue;

    public Stat(Func<float> baseValueProvider)
    {
        _baseValueProvider = baseValueProvider;
        _isDirty = true;
    }

    public float Value
    {
        get
        {
            if (_isDirty)
            {
                _lastValue = CalculateFinalValue();
                _isDirty = false;
            }
            return _lastValue;
        }
    }

    public void AddModifier(StatModifier mod)
    {
        _modifiers.Add(mod);
        _modifiers.Sort(CompareModifierOrder);
        _isDirty = true;
    }

    public bool RemoveModifier(StatModifier mod)
    {
        if (_modifiers.Remove(mod))
        {
            _isDirty = true;
            return true;
        }
        return false;
    }

    public bool RemoveAllModifiersFromSource(object source)
    {
        int removedCount = _modifiers.RemoveAll(m => m.Source == source);
        if (removedCount > 0)
        {
            _isDirty = true;
            return true;
        }
        return false;
    }

    private int CompareModifierOrder(StatModifier a, StatModifier b)
    {
        if (a.Type < b.Type) return -1;
        if (a.Type > b.Type) return 1;
        return 0;
    }

    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        for (int i = 0; i < _modifiers.Count; i++)
        {
            StatModifier mod = _modifiers[i];

            if (mod.Type == StatModifierType.Flat)
            {
                finalValue += mod.Value;
            }
            else if (mod.Type == StatModifierType.PercentAdd)
            {
                sumPercentAdd += mod.Value;
                
                if (i + 1 >= _modifiers.Count || _modifiers[i + 1].Type != StatModifierType.PercentAdd)
                {
                    finalValue *= (1 + sumPercentAdd);
                }
            }
            else if (mod.Type == StatModifierType.PercentMult)
            {
                finalValue *= mod.Value;
            }
        }

        return (float)Math.Round(finalValue, 4);
    }
}
