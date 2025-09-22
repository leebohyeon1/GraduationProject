using System;
using BH_Lib.Log;
using Unity.Collections;
using UnityEngine;

public class HeatSystem : MonoBehaviour, IHeatable
{
    [SerializeField] private int _maxHeat = 100;
    [SerializeField] private int _currentHeat = 0;
    private bool _isHeatLock = false;
    [SerializeField] protected SourceMapDatabaseSO p_sourceMapDataBase;
    [SerializeField] protected TierStatDatabaseSO p_tierStatDatabase;

    public event Action<int, int> OnHeatChanged;
    public event Action<int, int> OnTierChanged;

    public int MaxHeat => _maxHeat;
    public int CurrentHeat => _currentHeat;
    public int CurrentTier => GetTier();
    public bool IsHeatLock => _isHeatLock;
    public SourceMapDatabaseSO SourceMapDataBase => p_sourceMapDataBase;


    [field: SerializeField]
    public ActorType ActorType { get; private set; }
    public virtual void Init(ActorType actorType)
    {
        ActorType = actorType;
    }


    /// <summary>
    /// 열량 변경 함수
    /// </summary>
    /// <param name="amount"> 열기 변화량 </param>
    public void ChangeHeat(int amount)
    {
        if (amount == 0) return;

        int previousTier = GetTier();
        int previousHeat = _currentHeat;

        _currentHeat = Mathf.Clamp(_currentHeat + amount, 0, _maxHeat);

        if (previousHeat != _currentHeat)
        {
            OnHeatChanged?.Invoke(previousHeat, _currentHeat);
        }

        int newTier = GetTier();
        if (previousTier != newTier)
        {
            OnTierChanged?.Invoke(previousTier, newTier);
        }
        
        if (_currentHeat >= _maxHeat && !_isHeatLock)
        {
            Debug.Log("과열 발생");
            OverHeat();
            SetHeatLock(true);
        }   
    }

    /// <summary>
    /// 열량 설정 함수
    /// </summary>
    /// <param name="amount"> 열기 설정값 </param>
    public void SetHeat(int amount)
    {
        int previousTier = GetTier();
        int previousHeat = _currentHeat;

        _currentHeat = Mathf.Clamp(amount, 0, _maxHeat);

        if (previousHeat != _currentHeat)
        {
            OnHeatChanged?.Invoke(previousHeat, _currentHeat);
        }

        int newTier = GetTier();
        if (previousTier != newTier)
        {
            OnTierChanged?.Invoke(previousTier, newTier);
        }
    }

    protected CalculationResult CalculationHeat(string id, ActorType actorType, int tier, int baseDamage)
    {
        SourceMap data = p_sourceMapDataBase.GetSourceMap(id, actorType, tier);
        CalculationResult finalStats = StatCalculator.CalculateStats(data, baseDamage);
        return finalStats;
    }

    public int GetTier()
    {
        return p_tierStatDatabase.GetCurrentTier(_currentHeat);
    }

    public void SetHeatLock(bool isLock)
    {
        _isHeatLock = isLock;
    }
    protected virtual void OverHeat(){}
}
