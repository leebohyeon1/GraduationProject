using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 스테미나 UI
/// </summary>
public class PlayerStaminaUI : MonoBehaviour, IEventListener<PlayerController>, IDisposable
{
    [Header("References")]
    [SerializeField] private Image _plusStaminaBarImage;            // 양 스테미나바 이미지
    [SerializeField] private Image _minusStaminaBarImage;           // 음 스테미나바 이미지
    private PlayerController _playerController;                     // 플레이어

    [Header("Animation Setting")]
    [SerializeField] private float _animationSpeed = 0.3f;          // 애니메이션 속도
    [SerializeField] private AnimationCurve _animationCurve;        // 애니메이션 커브


    [SerializeField] private OnPlayerSpawnedSO _onPlayerSpawned;    // 플레이어 스폰 이벤트

    private void OnEnable() 
    {
        _onPlayerSpawned.Subscribe(this);
    }

    private void OnDisable()
    {
        _onPlayerSpawned.Unsubscribe(this);
    }

    /// <summary>
    /// 플레이어 스폰 이벤트 처리
    /// </summary>
    /// <param name="player">플레이어</param>
    public void OnEventTrigger(PlayerController player)
    {
        _playerController = player;

        _playerController.Stamina.OnStaminaChanged += OnStaminaChanged;

        OnStaminaChanged(_playerController.Stamina.CurrentStamina, _playerController.Stamina.CurrentStamina);

        player.RegisterDisposable(this);
    }

    /// <summary>
    /// 객체 해제
    /// </summary>
    public void Dispose()
    {
        _playerController.Stamina.OnStaminaChanged -= OnStaminaChanged;
    }

    // 체력 변경 이벤트 처리
    private void OnStaminaChanged(float previouseStamina, float currentStamina)
    {
        float currentfillAmount = previouseStamina / _playerController.Stamina.MaxStamina;
        DOTween.To(
            () => currentfillAmount,
            x =>
            {
                if(currentfillAmount > 0)
                {
                    _plusStaminaBarImage.fillAmount = currentfillAmount;
                    _minusStaminaBarImage.fillAmount = 0f;
                }
                else
                {
                    _plusStaminaBarImage.fillAmount = 0f;
                    _minusStaminaBarImage.fillAmount = Mathf.Abs(currentfillAmount);
                }

                currentfillAmount = x;
            },
            currentStamina / _playerController.Stamina.MaxStamina,
            _animationSpeed)
            .SetEase(_animationCurve);
    }
}