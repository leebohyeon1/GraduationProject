using System;
using System.Collections;
using UnityEngine;

public class PlayerStamina : MonoBehaviour, IDisposable
{
    private PlayerData _runtimeData;
    private PlayerEvents _events; // 플레이어 이벤트

    // private float _staminaRegenPerSecond;

    private Coroutine _regenStaminaCoroutine;
    [SerializeField] private float _regenDelay = 0.5f; // 회복 시작 전 지연 시간

    public event Action<float, float> OnStaminaChanged;

    #region Properties
    public float CurrentStamina => _runtimeData != null ? _runtimeData.CurrentStamina : 0;
    public float MaxStamina => _runtimeData != null ? _runtimeData.Stamina.Value : 100;
    public float StaminaRegenPerSecond => _runtimeData != null ? _runtimeData.StaminaRegenPerSecond.Value : 5f;
    #endregion

    /// <summary>
    /// 초기화 함수
    /// </summary>
    public void Initialize(PlayerController player)
    {
        _runtimeData = player.RuntimeData;
        _events = player.Events;

        _events.RegenStamina += OnRegenStamina;

        // 이벤트 해제 구독
        player.RegisterDisposable(this);
        StartCoroutine(RegenStamina());
    }

    public void Dispose()
    {
        _events.RegenStamina -= OnRegenStamina;

        _regenStaminaCoroutine = null;
        OnStaminaChanged = null;
    }

    /// <summary>
    /// 스테미나 사용가능 여부
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool CheckStamina()
    {
        return CurrentStamina > 0;
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
        if (_runtimeData == null) return;

        float previousStamina = CurrentStamina; 
        
        // 데이터 직접 수정 ( ~ Max 로 제한)
        _runtimeData.CurrentStamina = Mathf.Min(CurrentStamina + amount, MaxStamina);

        if (previousStamina != CurrentStamina)
        {
            OnStaminaChanged?.Invoke(previousStamina, CurrentStamina);
        }
    }

    /// <summary>
    /// 스테미나 재생 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator RegenStamina()
    {
        // 회복 시작 전 잠시 대기
        yield return new WaitForSeconds(_regenDelay);

        while (true)
        {
            yield return null;

            ChangeStamina(StaminaRegenPerSecond * Time.deltaTime);

            if (CurrentStamina >= MaxStamina)
            {
                ChangeStamina(MaxStamina - CurrentStamina);
                _regenStaminaCoroutine = null;
                break;
            }
        }
    }


    //==========================================================================================================================
    // EventHandler ============================================================================================================
    //==========================================================================================================================

    #region EventHandlers
    /// <summary>
    /// 스테미나 재생 이벤트 핸들러
    /// </summary>
    /// <param name="canRegen">재생 여부</param>
    private void OnRegenStamina(bool canRegen)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

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
