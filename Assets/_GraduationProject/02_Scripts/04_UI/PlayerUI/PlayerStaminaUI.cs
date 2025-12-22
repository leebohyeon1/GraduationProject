using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaUI : MonoBehaviour
{
    [SerializeField] private PlayerStamina _playerStamina;
    [SerializeField] private Image _plusStaminaImage;
    [SerializeField] private Image _minusStaminaImage;

    [Tooltip("게이지가 차오르거나 줄어드는 속도 (초당 퍼센트)")]
    [SerializeField] private float _fillSpeed = 2.0f; // 1초에 2칸(200%) 정도 이동하는 속도

    private float _currentAmount = 0;

    private void OnEnable()
    {
        // 초기화: UI가 켜질 때 현재 스테미나로 즉시 설정
        _playerStamina.OnStaminaChanged += HandleStaminaChanged;
    }

    private void Start()
    {
        _currentAmount = _playerStamina.Stamina;
        UpdateImages(_playerStamina.Stamina);
    }

    private void OnDisable()
    {
        _playerStamina.OnStaminaChanged -= HandleStaminaChanged;
    }

    private void HandleStaminaChanged(float previousStamina, float currentStamina)
    {
        DOTween.Kill(this);

        DOTween.To(
            () => _currentAmount,
            X =>
            {
                _currentAmount = X;
                UpdateImages(_currentAmount);
            },
           currentStamina,
             _fillSpeed)
            .SetEase(Ease.Linear)
            .SetId(this);
    }

    // 실제 이미지를 채우는 로직 분리
    private void UpdateImages(float staminaValue)
    {
        float maxStamina = _playerStamina.MaxStamina;

        if (staminaValue >= 0)
        {
            // 양수 구간: Plus는 비율대로, Minus는 0
            _plusStaminaImage.fillAmount = staminaValue / maxStamina;
            _minusStaminaImage.fillAmount = 0f;
        }
        else
        {
            // 음수 구간: Plus는 0, Minus는 절대값 비율대로
            _plusStaminaImage.fillAmount = 0f;
            _minusStaminaImage.fillAmount = Mathf.Abs(staminaValue) / maxStamina;
        }
    }
}