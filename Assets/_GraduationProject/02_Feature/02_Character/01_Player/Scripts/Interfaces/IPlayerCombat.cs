using UnityEngine;

/// <summary>
/// 플레이어의 기본 공격을 제외한 전투 인터페이스
/// </summary>
public interface IPlayerCombat
{
    public bool CanCounterAttack { get; }
    public bool IsRest { get; }

    public int MaxMana { get; }
    public int CurrentMana { get; }

    public void TryParry();

    public void TryCounterAttack();

    public Transform ParryStartEffectPoint { get; }

    public Transform CounterAttackStartEffectPoint { get; } 
    public Transform FirstCounterAttackEffectPoint { get; }
    public Transform SecondCounterAttackEffectPoint { get; }
    public Transform CounterAttackFinishEffectPoint { get; }

    public Transform ChargeStartEffectPoint { get; }
    public Transform ChargeFinishEffectPoint { get; }
    public Transform ChargeAttackStartEffectPoint { get; }
    public Transform ChargeAttackEffectPoint { get; }
    public Transform ChargeAttackFinishEffectPoint { get; }
    
}
