using System;
using UnityEngine;

/// <summary>
/// 플레이어의 모든 이벤트를 관리하고 피드백을 재생하는 클래스입니다.
/// </summary>
public class PlayerEvents
{
    #region EventHandler
    public event Action<bool> BattleStateChaged; // 전투 상태 변경 이벤트

    /// <summary>
    /// 전투 상태 변경 이벤트를 발생시킵니다.
    /// </summary>
    public void TriggerBattleStateChanged(bool isBattleState)
    {
        BattleStateChaged?.Invoke(isBattleState);
    }


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
        TriggerRegenStamina(false);
    }

    /// <summary>
    /// 회피 종료 이벤트를 발생시키고 피드백을 재생합니다. 
    /// </summary>
    public void TriggerDodgeFinished()
    {
        DodgeFinished?.Invoke();
        TriggerRegenStamina(true);
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
    public event Action OnlyChargeAttackSucceded;   // 오직 차지 공격 성공
    public event Action<int> AttackRegained;        // 공격 흡혈
    public event Action ChangeNextCombatState;

    /// <summary>
    /// 공격 시작 피드백을 재생합니다.
    /// </summary>
    public void TriggerAttackStarted()
    {
        AttackStarted?.Invoke();
        TriggerRegenStamina(false);
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
        AttackRegained?.Invoke(amount);
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
    public event Action ChargeStarted, ChargeFinished;                 // 차지 시작 종료 이벤트
    public event Action<int> ChargeLevelCompleted;                     // 차지 레벨 완료 이벤트

    /// <summary>
    /// 차지 시작 이벤트 발행
    /// </summary>
    public void TriggerChargeStarted()
    {
        ChargeStarted?.Invoke(); 
    }

    /// <summary>
    /// 차지 종료 이벤트 발행
    /// </summary>
    public void TriggerChargeFinshed()
    {
        ChargeFinished?.Invoke();
    }

    /// <summary>
    /// 차지 레벨 완료 이벤트 발행
    /// </summary>
    /// <param name="chargeLevel">차지 레벨</param>
    public void TriggerChargeLevelCompleted(int chargeLevel)
    {
        ChargeLevelCompleted?.Invoke(chargeLevel);
    }
    #endregion

    //==========================================================================================================================
    // Counter =================================================================================================================
    //==========================================================================================================================

    #region Counter
    public event Action CounterWindowStarted, CounterWindowFinished; // 패링 수행 이벤트
    public event Action<Transform>  CounterSucceeded; // 패링 성공 이벤트

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
    public void TriggerCounterSucceeded(Transform transform)
    {
        CounterSucceeded?.Invoke(transform);
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

    #endregion
}
