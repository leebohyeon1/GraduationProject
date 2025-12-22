using System;
using System.Collections;
using UnityEngine;

public class PlayerStamina : MonoBehaviour, IDisposable
{
    private PlayerStats _stats; // 플레이어 스탯
    private PlayerEvents _events; // 플레이어 이벤트

    private Coroutine _regenStaminaCoroutine;

    public event Action<float, float> OnStaminaChanged;


    #region Properties
    public float Stamina => _stats.CurrentStamina;
    public float MaxStamina => _stats.RuntimeData.MaxStamina;
    #endregion

    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(PlayerStats data, PlayerEvents evets)
    {
        _stats = data;
        _events = evets;

        _events.DodgeStarted += OnDodgeStarted;
        _events.RegenStamina += OnRegenStamina;

    }

    public void Dispose()
    {
        _events.DodgeStarted -= OnDodgeStarted;
        _events.RegenStamina -= OnRegenStamina;
    }

    /// <summary>
    /// 스테미나 사용가능 여부
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool CheckStamina()
    {
        return Stamina > 0;
    }

    /// <summary>
    /// 스테미나 사용 함수
    /// </summary>
    /// <param name="amount">사용한 스테미나</param>
    public void UseStamina(float amount)
    {
        ChangeStamina(-amount);
    }

    /// <summary>
    /// 스테미나 변경 함수
    /// </summary>
    /// <param name="amount">스테미나 변경량</param>
    public void ChangeStamina(float amount)
    {
        float previousStamina = _stats.CurrentStamina; 
        
        _stats.CurrentStamina = Mathf.Min(_stats.CurrentStamina + amount, MaxStamina);

        if (previousStamina != Stamina)
        {
            OnStaminaChanged?.Invoke(previousStamina, Stamina);
        }
    }

    /// <summary>
    /// 스테미나 재생 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator RegenStamina()
    {
        while (true)
        {
            yield return null;

            ChangeStamina(_stats.RuntimeData.StaminaRegenPerSecond * Time.deltaTime);

            if (Stamina >= MaxStamina)
            {
                ChangeStamina(MaxStamina - Stamina);
                _regenStaminaCoroutine = null;
                break;
            }
        }
    }

    #region EventHandlers
    /// <summary>
    /// 구르기 시작 이벤트 핸들러
    /// </summary>
    private void OnDodgeStarted()
    {
        UseStamina(_stats.RuntimeData.CombatData.DodgeStamina);
    }    

    /// <summary>
    /// 스테미나 재생 이벤트 핸들러
    /// </summary>
    /// <param name="canRegen">재생 여부</param>
    private void OnRegenStamina(bool canRegen)
    {
        if (canRegen)
        {
            if (_regenStaminaCoroutine == null)
            {
                _regenStaminaCoroutine = StartCoroutine(RegenStamina());
            }
        }
        else
        {
            if (_regenStaminaCoroutine != null)
            {
                StopCoroutine(_regenStaminaCoroutine);
                _regenStaminaCoroutine = null;
            }
        }
    }
    #endregion
}
