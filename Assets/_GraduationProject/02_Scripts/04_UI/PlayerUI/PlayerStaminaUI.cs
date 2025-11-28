using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaUI : MonoBehaviour
{
    [SerializeField] private PlayerStamina _playerStamina;
    [SerializeField] private Image _staminaImage;

    private void OnEnable()
    {
        _playerStamina.OnStaminaChanged += HandleStaminaChanged;
    }

    private void OnDisable()
    {
        _playerStamina.OnStaminaChanged -= HandleStaminaChanged;
    }


    private void HandleStaminaChanged(float previousStamina, float currentStamina)
    {
        DOTween.Kill(this);

        DOTween.To(
            () => _staminaImage.fillAmount,
            X =>
            {
                _staminaImage.fillAmount = X;
            },
            currentStamina / _playerStamina.MaxStamina,
            0.3f)
            .SetEase(Ease.Linear)
            .SetId(this);
    }    
}
