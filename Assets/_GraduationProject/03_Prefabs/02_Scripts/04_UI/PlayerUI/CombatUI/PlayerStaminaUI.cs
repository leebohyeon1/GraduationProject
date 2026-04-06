using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 스테미나 UI
/// </summary>
public class PlayerStaminaUI : PlayerUIBase
{
    [Header("References")]
    [SerializeField] private Image _plusStaminaBarImage;            // 양 스테미나바 이미지
    [SerializeField] private Image _minusStaminaBarImage;           // 음 스테미나바 이미지

    [Header("Animation Setting")]
    [SerializeField] private float _animationSpeed = 0.3f;          // 애니메이션 속도
    [SerializeField] private AnimationCurve _animationCurve;        // 애니메이션 커브


    /// <summary>
    /// 초기화
    /// </summary>
    /// <param name="player">플레이어</param>
    public override void Initialize(PlayerController player)
    {
        base.Initialize(player);

        p_player.Stamina.OnStaminaChanged += OnStaminaChanged;

        OnStaminaChanged(p_player.Stamina.CurrentStamina,p_player.Stamina.CurrentStamina);
    }

    /// <summary>
    /// 객체 해제
    /// </summary>
    public override void Dispose()
    {
        p_player.Stamina.OnStaminaChanged -= OnStaminaChanged;
    }

    // 체력 변경 이벤트 처리
    private void OnStaminaChanged(float previouseStamina, float currentStamina)
    {
        float currentfillAmount = previouseStamina /    p_player.Stamina.MaxStamina;
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
            currentStamina /p_player.Stamina.MaxStamina,
            _animationSpeed)
            .SetEase(_animationCurve);
    }
}