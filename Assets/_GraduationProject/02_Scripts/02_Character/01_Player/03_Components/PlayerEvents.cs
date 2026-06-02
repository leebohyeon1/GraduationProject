using System;
using UnityEngine;

/// <summary>
/// 플레이어의 모든 이벤트를 관리하고 피드백을 재생하는 클래스입니다.
/// </summary>
public class PlayerEvents
{
    #region EventHandler

    //==========================================================================================================================
    // Dodge ===================================================================================================================
    //==========================================================================================================================

    #region Dodge
    public event Action DodgeStarted, DodgeFinished; // 회피 종료 이벤트

    /// <summary>
    /// 회피 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerDodgeStarted()
    {
        DodgeStarted?.Invoke();
    }

    /// <summary>
    /// 회피 종료 이벤트를 발생시키고 피드백을 재생합니다. 
    /// </summary>
    public void TriggerDodgeFinished()
    {
        DodgeFinished?.Invoke();
    }
    #endregion


    //==========================================================================================================================
    // BufferInput =============================================================================================================
    //==========================================================================================================================

    #region BufferInput
    public event Action BufferInputStarted, BufferInputEnded;           // 선입력 버퍼 이벤트

    /// <summary>
    /// 입력 버퍼 시작 이벤트 발행
    /// </summary>
    public void TriggerBufferInputStarted()
    {
        BufferInputStarted?.Invoke();
    }
    
    /// <summary>
    /// 입력 버퍼 종료 이벤트 발행
    /// </summary>
    public void TriggerBufferInputEnded()
    {
        BufferInputEnded?.Invoke();
    }

    #endregion

    //==========================================================================================================================
    // Attack ==================================================================================================================
    //==========================================================================================================================

    #region Attack
    public event Action AttackStarted, AttackPerformed, AttackFinished; // 공격 시작, 공격 수행, 공격 종료 이벤트
    
    public delegate void TargetDamageEventHandler(Transform target, Stat damageStat);
    public event TargetDamageEventHandler BeforeDamageCalculate;          // 공격이 적중하기 전 이벤트 (데미지 수정 가능)

    public event Action OnlyChargeAttackSucceded;   // 오직 차지 공격 성공
    public event Action<int> AttackRegained;        // 공격 흡혈
    public Func<int, int> FilterAttackRegain; // 공격 흡혈량 필터링
    public event Action ChangeNextCombatState;

    /// <summary>
    /// 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerAttackStarted()
    {
        AttackStarted?.Invoke();
    }

    /// <summary>
    /// 공격 수행 이벤트를 발생시킵니다.
    /// </summary>
    public void TriggerAttackPerformed()
    {
        AttackPerformed?.Invoke();
    }

    /// <summary>
    /// 공격을 끝내고 다음 행동으로 넘어갑니다. 
    /// </summary>
    public void TriggerAttackFinished()
    {
        AttackFinished?.Invoke();
    }

    /// <summary>
    /// 공격 적용 전 이벤트
    /// </summary>
    /// <param name="attackTransform">공격할 대상</param>
    /// <param name="damageData">데미지 데이터 (수정 가능)</param>
    /// <param name="damageStat">현재 공격의 데미지 Stat 객체</param>
    public void TriggerBeforeDamageCalculate(Transform attackTransform, Stat damageStat)
    {
        BeforeDamageCalculate?.Invoke(attackTransform, damageStat);
    }

    /// <summary>
    /// 상쇄 없이 차지 공격만 성공했을 때 이벤트
    /// </summary>
    public void TriggerOnlyChargeAttackSucceded()
    {
        OnlyChargeAttackSucceded?.Invoke();
    }

    /// <summary>
    /// 공격 흡혈 이벤트
    /// </summary>
    /// <param name="amount">회복 량</param>
    public void TriggerAttackRegained(int amount)
    {
        int finalAmount = amount;
        if (FilterAttackRegain != null)
        {
            foreach (Delegate filter in FilterAttackRegain.GetInvocationList())
            {
                finalAmount = (int)filter.DynamicInvoke(finalAmount);
            }
        }
        AttackRegained?.Invoke(finalAmount);
    }

    public void TriggerChangeNextCombatState()
    {
        ChangeNextCombatState?.Invoke();
    }

    #endregion

    //==========================================================================================================================
    // Charge ==================================================================================================================
    //==========================================================================================================================

    #region Charge
    public event Action ChargeStarted;                 // 차지 시작 종료 이벤트
    public event Action<bool> ChargeCompleted;                     // 차지 완료 이벤트

    /// <summary>
    /// 차지 시작 이벤트 발행
    /// </summary>
    public void TriggerChargeStarted()
    {
        ChargeStarted?.Invoke(); 
    }

    /// <summary>
    /// 차지 레벨 완료 이벤트 발행
    /// </summary>
    public void TriggerChargeCompleted(bool isCharge)
    {
        ChargeCompleted?.Invoke(isCharge);
    }
    #endregion

    //==========================================================================================================================
    // Counter =================================================================================================================
    //==========================================================================================================================

    #region Counter
    public event Action CounterWindowStarted, CounterWindowFinished; // 패링 수행 이벤트
    public event Action<Transform, AttackType>  CounterSucceeded; // 패링 성공 이벤트

    /// <summary>
    /// 패링 검사 시작 이벤트 발행
    /// </summary>
    public void TriggerCounterWindowStarted()
    {
        CounterWindowStarted?.Invoke();
    }

    /// <summary>
    /// 패링 검사 종료 이벤트 발행
    /// </summary>
    public void TriggerCounterWindowFinished()
    {
        CounterWindowFinished?.Invoke();
    }

    /// <summary>
    /// 패링 성공 이벤트를 발생시키고 피드백을 재생합니다.
    /// </summary>
    public void TriggerCounterSucceeded(Transform transform, AttackType type)
    {
        CounterSucceeded?.Invoke(transform, type);
    }

    #endregion

    //==========================================================================================================================
    // Health ==================================================================================================================
    //==========================================================================================================================
    
    public event Action<int> Heal;  // 회복 이벤트

    /// <summary>
    /// 회복 이벤트 발행
    /// </summary>
    /// <param name="healAmount">회복량</param>
    public void TriggerHeal(int healAmount)
    {
        Heal?.Invoke(healAmount);
    }

    //==========================================================================================================================
    // Damaged =================================================================================================================
    //==========================================================================================================================

    #region Damaged
    public event RefAction<PlayerDamageContext> BeforeDamaged;  // 데미지 받기 전 대리자
    public event Action<DamageData> Damaged;          // 데미지 상태로 변하는 이벤트
    public event Action Knockdown;                    // 기절 상태로 변하는 이벤트
                      
    /// <summary>
    /// 데미지 받기 전 이벤트 발행
    /// </summary>
    /// <param name="damageContext">받은 데미지 데이터</param>
    public void TriggerBeforeDamaged(ref PlayerDamageContext damageContext)
    {
        BeforeDamaged?.Invoke(ref damageContext);
    }

    /// <summary>
    /// 데미지 상태로 변하는 이벤트 발행
    /// </summary>
    /// <param name="damageData">받은 데미지 데이터</param>
    public void TriggerDamaged(DamageData damageData)
    {
        Damaged?.Invoke(damageData);
    }

    /// <summary>
    /// 기절 상태로 변하는 이벤트 발행
    /// </summary>
    public void TriggerKnockdown()
    {
        Knockdown?.Invoke();    
    }
    #endregion

    //==========================================================================================================================
    // Stamina =================================================================================================================
    //==========================================================================================================================

    #region Stamina
    public event Action<bool> RegenStamina; // 스테미나 회복 이벤트

    /// <summary>
    /// 스테미나 회복 이벤트 발행
    /// </summary>
    /// <param name="canRegen">회복 가능 여부</param>
    public void TriggerRegenStamina(bool canRegen)
    {
        RegenStamina?.Invoke(canRegen);
    }
    #endregion

    //==========================================================================================================================
    // BattleState =============================================================================================================
    //==========================================================================================================================

    #region BattleState
    public event Action<bool> BattleStateChanged; // 전투 상태 변경 이벤트

    /// <summary>
    /// 전투 상태 변경 이벤트 발행
    /// </summary>
    /// <param name="isBattle">전투 상태 여부</param>
    public void TriggerBattleStateChanged(bool isBattle)
    {
        BattleStateChanged?.Invoke(isBattle);
    }
    #endregion


    //==========================================================================================================================
    // Land ====================================================================================================================
    //==========================================================================================================================

    public event Action Landed;

    public void TriggerLanded()
    {
         Landed?.Invoke(); 
    }


    #endregion

    /// <summary>
    /// 모든 이벤트를 초기화하여 구독을 해제합니다.
    /// 플레이어 사망 시 메모리 누수 방지를 위해 호출합니다.
    /// </summary>
    public void ClearAllEvents()
    {
        // Dodge
        DodgeStarted = null;
        DodgeFinished = null;

        // BufferInput
        BufferInputStarted = null;
        BufferInputEnded = null;

        // Attack
        AttackStarted = null;
        AttackPerformed = null;
        AttackFinished = null;
        OnlyChargeAttackSucceded = null;
        AttackRegained = null;
        FilterAttackRegain = null;
        ChangeNextCombatState = null;

        // Charge
        ChargeStarted = null;
        ChargeCompleted = null;

        // Counter
        CounterWindowStarted = null;
        CounterWindowFinished = null;
        CounterSucceeded = null;

        // Health & Damage
        Heal = null;
        BeforeDamaged = null;
        Damaged = null;
        Knockdown = null;

        // Stamina
        RegenStamina = null;

        Debug.Log("PlayerEvents: 모든 플레이어 이벤트가 초기화되었습니다.");
    }
}
