using System;
using BH_Lib.Log;
using Unity.Collections;
using UnityEngine;

public class HeatSystem : MonoBehaviour, IHeatable
{
    [SerializeField] private int _maxHeat = 100;
    [SerializeField] protected int p_currentHeat = 0;
    private bool _isHeatLock = false;
    [SerializeField] protected SourceMapDatabaseSO p_sourceMapDataBase;
    [SerializeField] protected TierStatDatabaseSO p_tierStatDatabase;

    public event Action<int, int> OnHeatChanged;
    public event Action<int, int> OnTierChanged;

    public int MaxHeat => _maxHeat;
    public int CurrentHeat => p_currentHeat;
    public int CurrentTier => GetTier();
    public bool IsHeatLock => _isHeatLock;
    public SourceMapDatabaseSO SourceMapDataBase => p_sourceMapDataBase;
    [SerializeField] float LockTimer = 2;
    private float Timer;


    [field: SerializeField]
    public ActorType ActorType { get; private set; }
    public virtual void Init(ActorType actorType)
    {
        ActorType = actorType;
        SetHeatLock(false);
        SetHeat(0); 
        StatCalculator.Initialize(p_tierStatDatabase);
    }



    /// <summary>
    /// 열량 변경 함수
    /// </summary>
    /// <param name="amount"> 열기 변화량 </param>
    public virtual void ChangeHeat(int amount)
    {
        if (p_tierStatDatabase == null)
        {
            p_tierStatDatabase = StatCalculator.TierStatDatabase;

        }
        if (IsHeatLock)
        {
            if (Time.time >= LockTimer)
            {
                SetHeatLock(false);
            }
        }
        if (amount == 0 && IsHeatLock) return;

        int previousTier = GetTier();
        int previousHeat = p_currentHeat;

        p_currentHeat = Mathf.Clamp(p_currentHeat + amount, 0, _maxHeat);

        if (previousHeat != p_currentHeat)
        {
            TriggerOnHeatChanged(previousHeat);
        }

        int newTier = GetTier();
        if (previousTier != newTier)
        {
            TriggerOnTierChanged(previousTier);
        }

        if (p_currentHeat >= _maxHeat && !_isHeatLock)
        {
            OverHeat();
            SetHeatLock(true);
            Timer = Time.time + LockTimer; // 2초 동안 열기 잠금
        }
    }

    /// <summary>
    /// 열량 설정 함수
    /// </summary>
    /// <param name="amount"> 열기 설정값 </param>
    public virtual void SetHeat(int amount)
    {
        // if (IsHeatLock)
        // {
        //     if (Time.time >= Timer)
        //     {
        //         SetHeatLock(false);
        //     }
        // }
        int previousTier = GetTier();
        int previousHeat = p_currentHeat;

        p_currentHeat = Mathf.Clamp(amount, 0, _maxHeat);

        if (previousHeat != p_currentHeat)
        {
            TriggerOnHeatChanged(previousHeat);
        }

        int newTier = GetTier();
        if (previousTier != newTier)
        {
            TriggerOnTierChanged(previousTier);
        }

        if (p_currentHeat >= _maxHeat && !_isHeatLock)
        {
            Debug.Log("과열 발생");
            OverHeat();
            SetHeatLock(true);
            Timer = Time.time + LockTimer; // 2초 동안 열기 잠금
        }
    }

    public CalculationResult CalculationHeat(string id, ActorType actorType, int tier, int baseDamage)
    {
        SourceMap data = p_sourceMapDataBase.GetSourceMap(id, actorType, tier);
        CalculationResult finalStats = StatCalculator.CalculateStats(data, baseDamage);
        return finalStats;
    }

    public virtual int GetTier()
    {
        return p_tierStatDatabase.GetCurrentTier(p_currentHeat);
    }

    public void SetHeatLock(bool isLock)
    {
        _isHeatLock = isLock;
    }

    protected virtual void OverHeat(){}

    protected virtual void TriggerOnHeatChanged(int previousHeat)
    {
        OnHeatChanged?.Invoke(previousHeat, p_currentHeat);
    }

    protected virtual void TriggerOnTierChanged(int previousTier)
    {
        OnTierChanged?.Invoke(previousTier, CurrentTier);
    }
}
